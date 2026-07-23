import { create } from 'zustand';
import { devtools } from 'zustand/middleware';
import api from '../services/api';

export interface Device {
  id: string;
  name: string;
  type: string;
  macAddress?: string;
  ipAddress?: string;
  status: string;
  battery?: number;
  firmware?: string;
  lastSeen?: string;
  location?: string;
  tags?: string[];
}

export interface Sensor extends Device {
  sensorType: string;
  currentValue?: number;
  lastUpdate?: string;
  signalStrength?: number;
  batteryLevel?: number;
}

export interface DeviceStoreState {
  devices: Device[];
  sensors: Sensor[];
  relays: Device[];
  loading: boolean;
  error?: string;
  
  // Actions
  initializeStore: () => Promise<void>;
  addDevice: (device: Device) => void;
  updateDevice: (deviceId: string, updates: Partial<Device>) => void;
  removeDevice: (deviceId: string) => void;
  addSensor: (sensor: Sensor) => void;
  updateSensor: (sensorId: string, updates: Partial<Sensor>) => void;
  setLoading: (loading: boolean) => void;
  setError: (error?: string) => void;
}

const useDeviceStore = create<DeviceStoreState>()(
  devtools(
    (set, get) => ({
      devices: [],
      sensors: [],
      relays: [],
      loading: false,
      error: undefined,

      initializeStore: async () => {
        set({ loading: true, error: undefined });
        try {
          const [devicesRes, sensorsRes] = await Promise.all([
            api.get('/api/devices'),
            api.get('/api/sensors'),
          ]);

          set({
            devices: devicesRes.data,
            sensors: sensorsRes.data,
            relays: devicesRes.data.filter((d: Device) => d.type === 'Relay'),
            loading: false,
          });
        } catch (error) {
          const errorMessage = error instanceof Error ? error.message : 'Failed to load devices';
          set({ error: errorMessage, loading: false });
        }
      },

      addDevice: (device) => {
        set((state) => ({
          devices: [...state.devices, device],
          relays: device.type === 'Relay' 
            ? [...state.relays, device] 
            : state.relays,
        }));
      },

      updateDevice: (deviceId, updates) => {
        set((state) => ({
          devices: state.devices.map((d) =>
            d.id === deviceId ? { ...d, ...updates } : d
          ),
          relays: state.relays.map((d) =>
            d.id === deviceId ? { ...d, ...updates } : d
          ),
        }));
      },

      removeDevice: (deviceId) => {
        set((state) => ({
          devices: state.devices.filter((d) => d.id !== deviceId),
          relays: state.relays.filter((d) => d.id !== deviceId),
        }));
      },

      addSensor: (sensor) => {
        set((state) => ({
          sensors: [...state.sensors, sensor],
        }));
      },

      updateSensor: (sensorId, updates) => {
        set((state) => ({
          sensors: state.sensors.map((s) =>
            s.id === sensorId ? { ...s, ...updates } : s
          ),
        }));
      },

      setLoading: (loading) => {
        set({ loading });
      },

      setError: (error) => {
        set({ error });
      },
    }),
    { name: 'DeviceStore' }
  )
);

export default useDeviceStore;

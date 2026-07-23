import { useEffect, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';
import useDeviceStore from '../store/deviceStore';

const SIGNALR_URL = import.meta.env.VITE_SIGNALR_URL || 'http://localhost:5000/signalr/devices';

let connection: signalR.HubConnection | null = null;

const useSignalRConnection = (isAuthenticated: boolean) => {
  const { updateDevice, updateSensor, addDevice } = useDeviceStore();

  // Handle sensor updates
  const handleSensorUpdated = useCallback(
    (update: {
      deviceId: string;
      sensorType: string;
      currentValue: number;
      batteryLevel?: number;
      signalStrength?: number;
      lastUpdate: string;
    }) => {
      updateSensor(update.deviceId, {
        currentValue: update.currentValue,
        batteryLevel: update.batteryLevel,
        signalStrength: update.signalStrength,
        lastUpdate: update.lastUpdate,
      });
    },
    [updateSensor]
  );

  // Handle relay state changes
  const handleRelayStateChanged = useCallback(
    (update: {
      relayId: string;
      deviceId: string;
      state: string;
      reason?: string;
      timestamp: string;
    }) => {
      updateDevice(update.relayId, {
        status: update.state.toUpperCase(),
      });
    },
    [updateDevice]
  );

  // Handle device status changes
  const handleDeviceStatusChanged = useCallback(
    (update: {
      deviceId: string;
      deviceName: string;
      status: string;
    }) => {
      updateDevice(update.deviceId, {
        status: update.status,
      });
    },
    [updateDevice]
  );

  // Initialize connection
  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    const initializeConnection = async () => {
      try {
        const token = localStorage.getItem('authToken');

        connection = new signalR.HubConnectionBuilder()
          .withUrl(SIGNALR_URL, {
            accessTokenFactory: () => token || '',
          })
          .withAutomaticReconnect([0, 0, 0, 3000, 5000, 10000])
          .configureLogging(signalR.LogLevel.Information)
          .build();

        // Set up event handlers
        connection.on('SensorUpdated', handleSensorUpdated);
        connection.on('RelayStateChanged', handleRelayStateChanged);
        connection.on('DeviceStatusChanged', handleDeviceStatusChanged);

        connection.onreconnected = () => {
          console.log('SignalR reconnected');
        };

        connection.onreconnecting = (error) => {
          console.warn('SignalR reconnecting:', error);
        };

        connection.onclose = (error) => {
          console.error('SignalR closed:', error);
        };

        await connection.start();
        console.log('SignalR connected');
      } catch (error) {
        console.error('Failed to connect to SignalR:', error);
        // Retry after 3 seconds
        setTimeout(initializeConnection, 3000);
      }
    };

    initializeConnection();

    return () => {
      if (connection?.state === signalR.HubConnectionState.Connected) {
        connection.stop();
      }
    };
  }, [isAuthenticated, handleSensorUpdated, handleRelayStateChanged, handleDeviceStatusChanged]);

  return connection;
};

export default useSignalRConnection;

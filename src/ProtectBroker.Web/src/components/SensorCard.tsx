import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  LinearProgress,
  Chip,
  Stack,
  Grid,
} from '@mui/material';
import {
  SignalCellularAlt as SignalIcon,
  BatteryFull as BatteryIcon,
  AccessTime as TimeIcon,
  Thermostat as TempIcon,
} from '@mui/icons-material';
import { Sensor } from '../store/deviceStore';

interface SensorCardProps {
  sensor: Sensor;
}

const SensorCard: React.FC<SensorCardProps> = ({ sensor }) => {
  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'online':
        return 'success';
      case 'offline':
        return 'error';
      case 'warning':
        return 'warning';
      default:
        return 'default';
    }
  };

  const getStatusChip = (status: string) => {
    const isOnline = status.toLowerCase() === 'online';
    return (
      <Chip
        label={status}
        color={getStatusColor(status)}
        variant="outlined"
        size="small"
      />
    );
  };

  const getSensorIcon = (sensorType?: string) => {
    if (!sensorType) return null;

    switch (sensorType.toLowerCase()) {
      case 'temperature':
        return <TempIcon sx={{ mr: 1 }} />;
      case 'motion':
        return <SignalIcon sx={{ mr: 1 }} />;
      default:
        return <TempIcon sx={{ mr: 1 }} />;
    }
  };

  const formatLastUpdate = (lastUpdate?: string) => {
    if (!lastUpdate) return 'Never';

    const date = new Date(lastUpdate);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins}m ago`;

    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours}h ago`;

    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays}d ago`;
  };

  return (
    <Card
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        cursor: 'pointer',
        transition: 'all 0.3s ease',
        '&:hover': {
          boxShadow: (theme) =>
            theme.palette.mode === 'dark'
              ? '0 8px 16px rgba(0,0,0,0.5)'
              : '0 8px 16px rgba(0,0,0,0.15)',
          transform: 'translateY(-4px)',
        },
      }}
    >
      <CardContent sx={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', mb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', flex: 1 }}>
            {getSensorIcon(sensor.sensorType)}
            <Typography variant="h6" sx={{ fontWeight: 600, wordBreak: 'break-word' }}>
              {sensor.name}
            </Typography>
          </Box>
        </Box>

        <Typography variant="caption" color="textSecondary" sx={{ mb: 1 }}>
          {sensor.sensorType || 'Unknown Type'}
        </Typography>

        <Box sx={{ mb: 2 }}>
          {getStatusChip(sensor.status)}
        </Box>

        {/* Sensor Value Display */}
        {sensor.currentValue !== undefined && (
          <Box sx={{ mb: 2, p: 1.5, bgcolor: 'action.hover', borderRadius: 1 }}>
            <Typography variant="body2" color="textSecondary" sx={{ mb: 0.5 }}>
              Current Value
            </Typography>
            <Typography variant="h5" sx={{ fontWeight: 700 }}>
              {sensor.currentValue.toFixed(1)}
              {sensor.sensorType?.toLowerCase().includes('temp') && ' °C'}
              {sensor.sensorType?.toLowerCase().includes('humidity') && ' %'}
            </Typography>
          </Box>
        )}

        {/* Battery and Signal */}
        <Grid container spacing={1} sx={{ mb: 2 }}>
          {sensor.batteryLevel !== undefined && (
            <Grid item xs={12}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <BatteryIcon sx={{ fontSize: '1.2rem' }} />
                <Box sx={{ flex: 1 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                    <Typography variant="caption">Battery</Typography>
                    <Typography variant="caption" sx={{ fontWeight: 600 }}>
                      {sensor.batteryLevel}%
                    </Typography>
                  </Box>
                  <LinearProgress
                    variant="determinate"
                    value={sensor.batteryLevel}
                    sx={{
                      height: 4,
                      borderRadius: 2,
                      backgroundColor: 'action.hover',
                      '& .MuiLinearProgress-bar': {
                        backgroundColor:
                          sensor.batteryLevel > 50
                            ? '#4caf50'
                            : sensor.batteryLevel > 20
                            ? '#ff9800'
                            : '#f44336',
                      },
                    }}
                  />
                </Box>
              </Box>
            </Grid>
          )}

          {sensor.signalStrength !== undefined && (
            <Grid item xs={12}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <SignalIcon sx={{ fontSize: '1.2rem' }} />
                <Typography variant="caption">
                  Signal: {sensor.signalStrength} dBm
                </Typography>
              </Box>
            </Grid>
          )}
        </Grid>

        {/* Last Update */}
        <Stack direction="row" spacing={0.5} sx={{ mt: 'auto', pt: 1, borderTop: '1px solid', borderColor: 'divider' }}>
          <TimeIcon sx={{ fontSize: '1rem', opacity: 0.7 }} />
          <Typography variant="caption" color="textSecondary">
            {formatLastUpdate(sensor.lastUpdate)}
          </Typography>
        </Stack>
      </CardContent>
    </Card>
  );
};

export default SensorCard;

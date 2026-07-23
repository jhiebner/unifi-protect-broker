import React, { useMemo } from 'react';
import {
  Container,
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  TextField,
  Button,
  Chip,
  Stack,
} from '@mui/material';
import { useState } from 'react';
import useDeviceStore from '../store/deviceStore';
import SensorCard from '../components/SensorCard';

const SensorsPage: React.FC = () => {
  const { sensors, loading } = useDeviceStore();
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<string | null>(null);

  const filteredSensors = useMemo(() => {
    return sensors.filter((sensor) => {
      const matchesSearch = sensor.name.toLowerCase().includes(searchTerm.toLowerCase());
      const matchesStatus = !filterStatus || sensor.status === filterStatus;
      return matchesSearch && matchesStatus;
    });
  }, [sensors, searchTerm, filterStatus]);

  const statusOptions = Array.from(new Set(sensors.map((s) => s.status)));

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" sx={{ mb: 3, fontWeight: 700 }}>
          Sensors
        </Typography>

        <Stack spacing={2} direction={{ xs: 'column', sm: 'row' }} sx={{ mb: 3 }}>
          <TextField
            placeholder="Search sensors..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            size="small"
            sx={{ flex: 1, maxWidth: { xs: '100%', sm: '300px' } }}
          />

          <Stack direction="row" spacing={1} sx={{ display: 'flex', alignItems: 'center' }}>
            {filterStatus && (
              <Button
                variant="outlined"
                size="small"
                onClick={() => setFilterStatus(null)}
              >
                Clear Filter
              </Button>
            )}
          </Stack>
        </Stack>

        <Stack direction="row" spacing={1} sx={{ mb: 3, flexWrap: 'wrap', gap: 1 }}>
          {statusOptions.map((status) => (
            <Chip
              key={status}
              label={`${status} (${sensors.filter((s) => s.status === status).length})`}
              onClick={() => setFilterStatus(filterStatus === status ? null : status)}
              variant={filterStatus === status ? 'filled' : 'outlined'}
              color={filterStatus === status ? 'primary' : 'default'}
            />
          ))}
        </Stack>
      </Box>

      {loading ? (
        <Box sx={{ textAlign: 'center', py: 4 }}>
          <Typography>Loading sensors...</Typography>
        </Box>
      ) : filteredSensors.length === 0 ? (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 4 }}>
            <Typography color="textSecondary">
              {sensors.length === 0
                ? 'No sensors found. Make sure UniFi Protect is connected.'
                : 'No sensors match your search.'}
            </Typography>
          </CardContent>
        </Card>
      ) : (
        <Grid container spacing={2}>
          {filteredSensors.map((sensor) => (
            <Grid item xs={12} sm={6} md={4} lg={3} key={sensor.id}>
              <SensorCard sensor={sensor} />
            </Grid>
          ))}
        </Grid>
      )}
    </Container>
  );
};

export default SensorsPage;

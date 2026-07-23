import React from 'react';
import { Container, Box, Typography, Grid, Card, CardContent } from '@mui/material';
import useDeviceStore from '../store/deviceStore';

const DashboardPage: React.FC = () => {
  const { devices, sensors, relays, loading } = useDeviceStore();

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" sx={{ mb: 3, fontWeight: 700 }}>
          Dashboard
        </Typography>
      </Box>

      {loading ? (
        <Typography>Loading...</Typography>
      ) : (
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary">Total Devices</Typography>
                <Typography variant="h4" sx={{ mt: 1 }}>
                  {devices.length}
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary">Active Sensors</Typography>
                <Typography variant="h4" sx={{ mt: 1 }}>
                  {sensors.filter((s) => s.status === 'Online').length}
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary">Relays</Typography>
                <Typography variant="h4" sx={{ mt: 1 }}>
                  {relays.length}
                </Typography>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary">Alerts</Typography>
                <Typography variant="h4" sx={{ mt: 1 }}>
                  0
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}
    </Container>
  );
};

export default DashboardPage;

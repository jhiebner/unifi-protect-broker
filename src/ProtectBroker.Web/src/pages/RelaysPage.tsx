import React from 'react';
import { Container, Box, Typography, Grid, Card, CardContent } from '@mui/material';
import useDeviceStore from '../store/deviceStore';

const RelaysPage: React.FC = () => {
  const { relays, loading } = useDeviceStore();

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" sx={{ mb: 3, fontWeight: 700 }}>
          Relay Controls
        </Typography>
      </Box>

      {loading ? (
        <Typography>Loading...</Typography>
      ) : relays.length === 0 ? (
        <Card>
          <CardContent>
            <Typography color="textSecondary">No relays found</Typography>
          </CardContent>
        </Card>
      ) : (
        <Grid container spacing={2}>
          {relays.map((relay) => (
            <Grid item xs={12} sm={6} md={4} key={relay.id}>
              <Card>
                <CardContent>
                  <Typography variant="h6">{relay.name}</Typography>
                  <Typography color="textSecondary" variant="body2">
                    Status: {relay.status}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}
    </Container>
  );
};

export default RelaysPage;

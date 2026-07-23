import React from 'react';
import { Container, Box, Typography } from '@mui/material';

const SettingsPage: React.FC = () => {
  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" sx={{ mb: 3, fontWeight: 700 }}>
          Settings
        </Typography>
        <Typography color="textSecondary">Settings page coming soon...</Typography>
      </Box>
    </Container>
  );
};

export default SettingsPage;

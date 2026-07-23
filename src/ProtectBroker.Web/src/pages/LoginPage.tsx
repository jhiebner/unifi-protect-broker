import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Card,
  TextField,
  Button,
  Alert,
  Box,
  Typography,
  CircularProgress,
} from '@mui/material';
import api from '../services/api';

interface LoginPageProps {
  onLoginSuccess: () => void;
}

const LoginPage: React.FC<LoginPageProps> = ({ onLoginSuccess }) => {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      const response = await api.post('/api/auth/login', {
        email,
        password,
      });

      const { token } = response.data;
      localStorage.setItem('authToken', token);
      
      onLoginSuccess();
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Login failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      }}
    >
      <Container maxWidth="sm">
        <Card sx={{ p: 4 }}>
          <Box sx={{ textAlign: 'center', mb: 3 }}>
            <Typography variant="h3" sx={{ fontWeight: 700, mb: 1 }}>
              Protect Broker
            </Typography>
            <Typography variant="body1" color="textSecondary">
              Farm Automation Dashboard
            </Typography>
          </Box>

          <form onSubmit={handleLogin}>
            {error && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {error}
              </Alert>
            )}

            <TextField
              fullWidth
              label="Email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              margin="normal"
              disabled={loading}
              required
            />

            <TextField
              fullWidth
              label="Password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              margin="normal"
              disabled={loading}
              required
            />

            <Button
              fullWidth
              variant="contained"
              size="large"
              type="submit"
              sx={{ mt: 3 }}
              disabled={loading}
            >
              {loading ? <CircularProgress size={24} /> : 'Sign In'}
            </Button>
          </form>

          <Typography variant="caption" sx={{ display: 'block', mt: 3, textAlign: 'center' }}>
            Default credentials: admin@example.com / Admin123!
          </Typography>
        </Card>
      </Container>
    </Box>
  );
};

export default LoginPage;

import React, { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, createTheme, CssBaseline, Box } from '@mui/material';
import useDarkMode from './hooks/useDarkMode';
import useDeviceStore from './store/deviceStore';
import useSignalRConnection from './hooks/useSignalRConnection';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import SensorsPage from './pages/SensorsPage';
import RelaysPage from './pages/RelaysPage';
import SettingsPage from './pages/SettingsPage';
import Layout from './components/Layout';

const App: React.FC = () => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const { isDark, toggleDarkMode } = useDarkMode();
  const { initializeStore } = useDeviceStore();

  // SignalR connection hook
  useSignalRConnection(isAuthenticated);

  // Check authentication on app load
  useEffect(() => {
    const checkAuth = () => {
      const token = localStorage.getItem('authToken');
      setIsAuthenticated(!!token);
      setIsLoading(false);
    };

    checkAuth();
  }, []);

  // Initialize device store when authenticated
  useEffect(() => {
    if (isAuthenticated) {
      initializeStore();
    }
  }, [isAuthenticated, initializeStore]);

  // Create theme based on dark mode
  const theme = createTheme({
    palette: {
      mode: isDark ? 'dark' : 'light',
      primary: {
        main: '#1976d2',
      },
      secondary: {
        main: '#dc004e',
      },
      background: {
        default: isDark ? '#121212' : '#fafafa',
        paper: isDark ? '#1e1e1e' : '#ffffff',
      },
    },
    typography: {
      fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
      h1: {
        fontSize: '2.5rem',
        fontWeight: 700,
      },
      h2: {
        fontSize: '2rem',
        fontWeight: 600,
      },
      button: {
        textTransform: 'none',
        fontWeight: 600,
      },
    },
    components: {
      MuiButton: {
        styleOverrides: {
          root: {
            borderRadius: '8px',
            padding: '10px 20px',
            fontSize: '1rem',
          },
          sizeLarge: {
            padding: '14px 28px',
            fontSize: '1.1rem',
          },
        },
      },
      MuiCard: {
        styleOverrides: {
          root: {
            borderRadius: '12px',
            boxShadow: isDark 
              ? '0 2px 8px rgba(0,0,0,0.3)' 
              : '0 2px 8px rgba(0,0,0,0.1)',
          },
        },
      },
    },
  });

  if (isLoading) {
    return (
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Box 
          display="flex" 
          justifyContent="center" 
          alignItems="center" 
          minHeight="100vh"
        >
          <div>Loading...</div>
        </Box>
      </ThemeProvider>
    );
  }

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Router>
        <Routes>
          <Route
            path="/login"
            element={
              isAuthenticated ? (
                <Navigate to="/" replace />
              ) : (
                <LoginPage onLoginSuccess={() => setIsAuthenticated(true)} />
              )
            }
          />

          {isAuthenticated ? (
            <Route element={<Layout isDark={isDark} onToggleDarkMode={toggleDarkMode} />}>
              <Route path="/" element={<DashboardPage />} />
              <Route path="/sensors" element={<SensorsPage />} />
              <Route path="/relays" element={<RelaysPage />} />
              <Route path="/settings" element={<SettingsPage />} />
            </Route>
          ) : (
            <Route path="*" element={<Navigate to="/login" replace />} />
          )}
        </Routes>
      </Router>
    </ThemeProvider>
  );
};

export default App;

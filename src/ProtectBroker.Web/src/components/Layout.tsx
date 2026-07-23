import React, { useState } from 'react';
import { Outlet, Link, useNavigate } from 'react-router-dom';
import {
  AppBar,
  Toolbar,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  IconButton,
  Box,
  Typography,
  Divider,
} from '@mui/material';
import {
  Menu as MenuIcon,
  Close as CloseIcon,
  Dashboard as DashboardIcon,
  Sensors as SensorsIcon,
  ElectricalServices as RelaysIcon,
  Settings as SettingsIcon,
  Logout as LogoutIcon,
  Brightness4 as DarkIcon,
  Brightness7 as LightIcon,
} from '@mui/icons-material';

interface LayoutProps {
  isDark: boolean;
  onToggleDarkMode: () => void;
}

const Layout: React.FC<LayoutProps> = ({ isDark, onToggleDarkMode }) => {
  const navigate = useNavigate();
  const [drawerOpen, setDrawerOpen] = useState(false);

  const handleLogout = () => {
    localStorage.removeItem('authToken');
    navigate('/login');
  };

  const menuItems = [
    { label: 'Dashboard', icon: <DashboardIcon />, to: '/' },
    { label: 'Sensors', icon: <SensorsIcon />, to: '/sensors' },
    { label: 'Relays', icon: <RelaysIcon />, to: '/relays' },
    { label: 'Settings', icon: <SettingsIcon />, to: '/settings' },
  ];

  const drawer = (
    <Box sx={{ width: 280, display: 'flex', flexDirection: 'column', height: '100%' }}>
      <Box sx={{ p: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h6" sx={{ fontWeight: 700 }}>
          Protect Broker
        </Typography>
        <IconButton
          onClick={() => setDrawerOpen(false)}
          sx={{ display: { sm: 'none' } }}
        >
          <CloseIcon />
        </IconButton>
      </Box>

      <Divider />

      <List sx={{ flex: 1 }}>
        {menuItems.map((item) => (
          <ListItem key={item.label} disablePadding>
            <ListItemButton
              component={Link}
              to={item.to}
              onClick={() => setDrawerOpen(false)}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>

      <Divider />

      <List>
        <ListItem disablePadding>
          <ListItemButton onClick={onToggleDarkMode}>
            <ListItemIcon>
              {isDark ? <LightIcon /> : <DarkIcon />}
            </ListItemIcon>
            <ListItemText primary={isDark ? 'Light Mode' : 'Dark Mode'} />
          </ListItemButton>
        </ListItem>

        <ListItem disablePadding>
          <ListItemButton onClick={handleLogout}>
            <ListItemIcon>
              <LogoutIcon />
            </ListItemIcon>
            <ListItemText primary="Logout" />
          </ListItemButton>
        </ListItem>
      </List>
    </Box>
  );

  return (
    <Box sx={{ display: 'flex' }}>
      <AppBar position="fixed">
        <Toolbar>
          <IconButton
            color="inherit"
            onClick={() => setDrawerOpen(true)}
            sx={{ mr: 2, display: { md: 'none' } }}
          >
            <MenuIcon />
          </IconButton>

          <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 700 }}>
            Protect Broker
          </Typography>

          <IconButton color="inherit" onClick={onToggleDarkMode}>
            {isDark ? <LightIcon /> : <DarkIcon />}
          </IconButton>
        </Toolbar>
      </AppBar>

      <Drawer
        anchor="left"
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        sx={{ display: { xs: 'block', md: 'none' } }}
      >
        {drawer}
      </Drawer>

      <Box
        sx={{
          display: { xs: 'none', md: 'block' },
          width: 280,
          flexShrink: 0,
          mt: 0,
        }}
      >
        <Box
          sx={{
            p: 0,
            position: 'fixed',
            left: 0,
            top: 64,
            width: 280,
            height: 'calc(100vh - 64px)',
            borderRight: '1px solid',
            borderColor: 'divider',
            overflowY: 'auto',
          }}
        >
          {drawer}
        </Box>
      </Box>

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          mt: 8,
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        <Outlet />
      </Box>
    </Box>
  );
};

export default Layout;

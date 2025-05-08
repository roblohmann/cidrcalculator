// src/pages/Home.tsx
import React from 'react';
import { Container } from '@mui/material';
import { IpForm } from '../components/IpForm';

const Home: React.FC = () => (
  <Container maxWidth="md">
    <IpForm />
  </Container>
);

export default Home;

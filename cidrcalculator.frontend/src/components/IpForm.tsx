import React, { useState } from 'react';
import { TextField, Button, Box } from '@mui/material';

export const IpForm: React.FC = () => {
  const [cidr, setCidr] = useState('');
  const [result, setResult] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setResult(null);

    try {
      const response = await fetch('https://localhost:7089/api/ipinfo/range-info', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ CIDR: cidr }), // <-- zo stuur je de CIDR in de body
      });

      if (!response.ok) {
        const err = await response.text();
        throw new Error(err);
      }

      const data = await response.json();
      setResult(data);
    } catch (err: any) {
      setError(err.message || 'Er ging iets mis');
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2 }}>
      <TextField
        label="CIDR Range"
        value={cidr}
        onChange={(e) => setCidr(e.target.value)}
        fullWidth
        margin="normal"
        required
      />
      <Button type="submit" variant="contained">
        Bereken
      </Button>

      {error && <p style={{ color: 'red' }}>{error}</p>}
      {result && (
        <Box sx={{ mt: 2 }}>
          <pre>{JSON.stringify(result, null, 2)}</pre>
        </Box>
      )}
    </Box>
  );
};

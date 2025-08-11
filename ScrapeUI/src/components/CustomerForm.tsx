import React, { useState } from 'react';

interface Customer {
  name: string;
  email: string;
  phone: string;
}

const CustomerForm: React.FC = () => {
  const [form, setForm] = useState<Customer>({ name: '', email: '', phone: '' });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccess(false);

    try {
      const response = await fetch('http://localhost:5000/api/Customers', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(form),
      });
      if (!response.ok) {
        throw new Error('Failed to create customer');
      }
      setSuccess(true);
      setForm({ name: '', email: '', phone: '' });
    } catch (err: any) {
      setError(err.message || 'Unknown error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 400, margin: '2rem auto', padding: 24, border: '1px solid #ccc', borderRadius: 8, background: '#fafafa' }}>
      <h2 style={{ textAlign: 'center' }}>Create Customer</h2>
      <div style={{ marginBottom: 12 }}>
        <label>Name:</label>
        <input name="name" value={form.name} onChange={handleChange} required style={{ width: '100%', padding: 8, marginTop: 4 }} />
      </div>
      <div style={{ marginBottom: 12 }}>
        <label>Email:</label>
        <input name="email" value={form.email} onChange={handleChange} required type="email" style={{ width: '100%', padding: 8, marginTop: 4 }} />
      </div>
      <div style={{ marginBottom: 12 }}>
        <label>Phone:</label>
        <input name="phone" value={form.phone} onChange={handleChange} required style={{ width: '100%', padding: 8, marginTop: 4 }} />
      </div>
      <button type="submit" disabled={loading} style={{ width: '100%', padding: 10, background: '#1976d2', color: '#fff', border: 'none', borderRadius: 4 }}>
        {loading ? 'Creating...' : 'Create'}
      </button>
      {error && <div style={{ color: 'red', marginTop: 12 }}>{error}</div>}
      {success && <div style={{ color: 'green', marginTop: 12 }}>Customer created!</div>}
    </form>
  );
};

export default CustomerForm;

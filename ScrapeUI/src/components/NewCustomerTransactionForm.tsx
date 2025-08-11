import React, { useState, useEffect } from 'react';

interface Customer {
  id: number;
  name: string;
}

interface ProductEntry {
  productName: string;
  grossWeight: number;
  netWeight: number;
  rate: number;
  totalAmount: number;
}

interface CustomerTransactionForm {
  customerId: number;
  date: string;
  productEntries: ProductEntry[];
}

const NewCustomerTransactionForm: React.FC = () => {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [form, setForm] = useState<CustomerTransactionForm>({
    customerId: 0,
    date: '',
    productEntries: [{ productName: '', grossWeight: 0, netWeight: 0, rate: 0, totalAmount: 0 }],
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    // Fetch customers for dropdown
    const fetchCustomers = async () => {
      try {
        const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/Customers`);
        if (!response.ok) throw new Error('Failed to fetch customers');
        const data = await response.json();
        setCustomers(data);
      } catch (err: any) {
        setError(err.message || 'Unknown error');
      }
    };
    fetchCustomers();
  }, []);

  const handleProductChange = (idx: number, e: React.ChangeEvent<HTMLInputElement>) => {
    const newEntries = [...form.productEntries];
    newEntries[idx] = { ...newEntries[idx], [e.target.name]: e.target.value };
    setForm({ ...form, productEntries: newEntries });
  };

  const handleAddProduct = () => {
    setForm({ ...form, productEntries: [...form.productEntries, { productName: '', grossWeight: 0, netWeight: 0, rate: 0, totalAmount: 0 }] });
  };

  const handleRemoveProduct = (idx: number) => {
    const newEntries = form.productEntries.filter((_, i) => i !== idx);
    setForm({ ...form, productEntries: newEntries });
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setSuccess(false);
    try {
      const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/CustomerTransactions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ...form,
          productEntries: form.productEntries.map(pe => ({
            ...pe,
            grossWeight: Number(pe.grossWeight),
            netWeight: Number(pe.netWeight),
            rate: Number(pe.rate),
            totalAmount: Number(pe.totalAmount),
          })),
          customerId: Number(form.customerId),
        }),
      });
      if (!response.ok) throw new Error('Failed to create transaction');
      setSuccess(true);
      setForm({ customerId: 0, date: '', productEntries: [{ productName: '', grossWeight: 0, netWeight: 0, rate: 0, totalAmount: 0 }] });
    } catch (err: any) {
      setError(err.message || 'Unknown error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} style={{ maxWidth: 600, margin: '2rem auto', padding: 24, border: '1px solid #ccc', borderRadius: 8, background: '#fafafa' }}>
      <h2>Create Customer Transaction</h2>
      <div style={{ marginBottom: 12 }}>
        <label>Customer:</label>
        <select name="customerId" value={form.customerId} onChange={handleChange} required style={{ width: '100%', padding: 8, marginTop: 4 }}>
          <option value="">Select Customer</option>
          {customers.map(c => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </select>
      </div>
      <div style={{ marginBottom: 12 }}>
        <label>Date:</label>
        <input name="date" type="date" value={form.date} onChange={handleChange} required style={{ width: '100%', padding: 8, marginTop: 4 }} />
      </div>
      <h3>Product Entries</h3>
      {form.productEntries.map((pe, idx) => (
        <div key={idx} style={{ border: '1px solid #eee', padding: 12, marginBottom: 8, borderRadius: 4 }}>
          <input name="productName" placeholder="Product Name" value={pe.productName} onChange={e => handleProductChange(idx, e)} required style={{ marginRight: 8 }} />
          <input name="grossWeight" type="number" placeholder="Gross Weight" value={pe.grossWeight} onChange={e => handleProductChange(idx, e)} required style={{ marginRight: 8 }} />
          <input name="netWeight" type="number" placeholder="Net Weight" value={pe.netWeight} onChange={e => handleProductChange(idx, e)} required style={{ marginRight: 8 }} />
          <input name="rate" type="number" placeholder="Rate" value={pe.rate} onChange={e => handleProductChange(idx, e)} required style={{ marginRight: 8 }} />
          <input name="totalAmount" type="number" placeholder="Total Amount" value={pe.totalAmount} onChange={e => handleProductChange(idx, e)} required style={{ marginRight: 8 }} />
          {form.productEntries.length > 1 && <button type="button" onClick={() => handleRemoveProduct(idx)} style={{ marginLeft: 8 }}>Remove</button>}
        </div>
      ))}
      <button type="button" onClick={handleAddProduct} style={{ marginBottom: 16 }}>Add Product</button>
      <br />
      <button type="submit" disabled={loading} style={{ width: '100%', padding: 10, background: '#1976d2', color: '#fff', border: 'none', borderRadius: 4 }}>
        {loading ? 'Creating...' : 'Create Transaction'}
      </button>
      {error && <div style={{ color: 'red', marginTop: 12 }}>{error}</div>}
      {success && <div style={{ color: 'green', marginTop: 12 }}>Transaction created!</div>}
    </form>
  );
};

export default NewCustomerTransactionForm;

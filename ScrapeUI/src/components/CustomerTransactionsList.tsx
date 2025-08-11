import React, { useEffect, useState } from 'react';

interface ProductEntry {
  id: number;
  // Add other fields as needed
}

interface CustomerTransaction {
  id: number;
  customerId: number;
  date: string;
  productEntries: ProductEntry[];
  // Add other fields as needed
}

const CustomerTransactionsList: React.FC = () => {
  const [transactions, setTransactions] = useState<CustomerTransaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchTransactions = async () => {
      try {
        const response = await fetch(`${process.env.REACT_APP_API_BASE_URL}/api/CustomerTransactions`);
        if (!response.ok) {
          throw new Error('Failed to fetch transactions');
        }
        const data = await response.json();
        setTransactions(data);
      } catch (err: any) {
        setError(err.message || 'Unknown error');
      } finally {
        setLoading(false);
      }
    };
    fetchTransactions();
  }, []);

  if (loading) return <div>Loading transactions...</div>;
  if (error) return <div style={{ color: 'red' }}>{error}</div>;

  return (
    <div style={{ maxWidth: 800, margin: '2rem auto' }}>
      <h2>Customer Transactions</h2>
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr>
            <th style={{ border: '1px solid #ccc', padding: 8 }}>ID</th>
            <th style={{ border: '1px solid #ccc', padding: 8 }}>Customer ID</th>
            <th style={{ border: '1px solid #ccc', padding: 8 }}>Date</th>
            <th style={{ border: '1px solid #ccc', padding: 8 }}>Product Entries</th>
          </tr>
        </thead>
        <tbody>
          {transactions.map((tx) => (
            <tr key={tx.id}>
              <td style={{ border: '1px solid #ccc', padding: 8 }}>{tx.id}</td>
              <td style={{ border: '1px solid #ccc', padding: 8 }}>{tx.customerId}</td>
              <td style={{ border: '1px solid #ccc', padding: 8 }}>{new Date(tx.date).toLocaleDateString()}</td>
              <td style={{ border: '1px solid #ccc', padding: 8 }}>{tx.productEntries?.length ?? 0}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default CustomerTransactionsList;

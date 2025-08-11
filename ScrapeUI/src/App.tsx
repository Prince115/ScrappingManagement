import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import './styles/App.css';
import CustomerForm from './components/CustomerForm';
import CustomerList from './components/CustomerList';
import CustomerTransactionsList from './components/CustomerTransactionsList';
import NewCustomerTransactionForm from './components/NewCustomerTransactionForm';

const Home: React.FC = () => (
  <div>
    <h2>Home</h2>
    <p>Welcome to ScrapeUI!</p>
  </div>
);

const App: React.FC = () => {
  return (
    <Router>
      <nav style={{ padding: 16, background: '#eee', marginBottom: 24 }}>
        <Link to="/" style={{ marginRight: 16 }}>Home</Link>
        <Link to="/customer-form">Create Customer</Link>

        <Link to="/customers" style={{ marginLeft: 16 }}>Customer List</Link>
        <Link to="/transactions" style={{ marginLeft: 16 }}>Customer Transactions</Link>
        <Link to="/add-transaction" style={{ marginLeft: 16 }}>Add Transaction</Link>
      </nav>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/customer-form" element={<CustomerForm />} />
        <Route path="/customers" element={<CustomerList />} />
        <Route path="/transactions" element={<CustomerTransactionsList />} />
        <Route path="/add-transaction" element={<NewCustomerTransactionForm />} />
        {/* Add more routes here as needed */}
      </Routes>
    </Router>
  );
};

export default App;
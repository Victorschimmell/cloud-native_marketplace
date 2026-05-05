import { RouterProvider } from 'react-router-dom';
import { AuthProvider } from './features/auth/AuthProvider';
import { router } from './routes';
import { CurrencyProvider } from './shared/currency/CurrencyProvider';

function App() {
  return (
    <AuthProvider>
      <CurrencyProvider>
        <RouterProvider router={router} future={{ v7_startTransition: true }} />
      </CurrencyProvider>
    </AuthProvider>
  );
}

export default App;

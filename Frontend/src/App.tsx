import { RouterProvider } from 'react-router-dom';
import { router } from './routes';
import { CurrencyProvider } from './shared/currency/CurrencyProvider';

function App() {
  return (
    <CurrencyProvider>
      <RouterProvider router={router} />
    </CurrencyProvider>
  );
}

export default App;

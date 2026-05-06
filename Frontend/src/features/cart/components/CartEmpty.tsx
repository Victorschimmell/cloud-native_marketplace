import { Link } from 'react-router-dom';
import './CartEmpty.css';

export default function CartEmpty() {
    return (
        <div className="cart-empty">
            <h2>Your cart is empty</h2>
            <p>Continue shopping to add items to your cart.</p>
            <Link className="cart-empty__continue-shopping-link" to="/products">
                Continue shopping
            </Link>
        </div>
    );
}

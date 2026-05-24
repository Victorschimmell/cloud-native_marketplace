import { useEffect, useId } from 'react';
import type { ReactNode } from 'react';
import './Modal.css';

interface ModalProps {
  children: ReactNode;
  footer?: ReactNode;
  onClose: () => void;
  subtitle?: ReactNode;
  title: ReactNode;
}

export default function Modal({ children, footer, onClose, subtitle, title }: ModalProps) {
  const titleId = useId();

  useEffect(() => {
    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        onClose();
      }
    }

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  return (
    <div className="modal__backdrop" onClick={onClose}>
      <section
        aria-labelledby={titleId}
        aria-modal="true"
        className="modal"
        role="dialog"
        onClick={(event) => event.stopPropagation()}
      >
        <div className="modal__header">
          <div>
            <h3 className="modal__title" id={titleId}>{title}</h3>
            {subtitle ? <p className="modal__subtitle">{subtitle}</p> : null}
          </div>
          <button type="button" className="modal__close" aria-label="Close" onClick={onClose}>
            x
          </button>
        </div>

        <div className="modal__body">{children}</div>

        {footer ? <div className="modal__footer">{footer}</div> : null}
      </section>
    </div>
  );
}

import { useEffect, useRef } from 'react';
import { Modal as BSModal } from 'bootstrap';

export default function Modal({ id, title, children, onClose }) {
  const modalRef = useRef(null);
  const bsModal = useRef(null);

  useEffect(() => {
    if (modalRef.current) {
      bsModal.current = new BSModal(modalRef.current);
    }
    return () => bsModal.current?.dispose();
  }, []);

  return (
    <div className="modal fade" id={id} ref={modalRef} tabIndex="-1">
      <div className="modal-dialog modal-lg">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">{title}</h5>
            <button type="button" className="btn-close" data-bs-dismiss="modal"></button>
          </div>
          {children}
        </div>
      </div>
    </div>
  );
}

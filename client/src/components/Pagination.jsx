export default function Pagination({ page, pageSize, totalCount, onPage }) {
  const totalPages = Math.ceil(totalCount / pageSize);
  if (totalPages <= 1) return null;

  const pages = [];
  const delta = 2;
  for (let i = 1; i <= totalPages; i++) {
    if (i === 1 || i === totalPages || (i >= page - delta && i <= page + delta)) {
      pages.push(i);
    } else if (pages[pages.length - 1] !== '...') {
      pages.push('...');
    }
  }

  const from = (page - 1) * pageSize + 1;
  const to   = Math.min(page * pageSize, totalCount);

  return (
    <div className="d-flex align-items-center justify-content-between mt-3 px-1 flex-wrap gap-2">
      <small className="text-muted">
        מציג {from}–{to} מתוך {totalCount}
      </small>
      <nav>
        <ul className="pagination pagination-sm mb-0">
          <li className={`page-item ${page === 1 ? 'disabled' : ''}`}>
            <button className="page-link" onClick={() => onPage(page - 1)}>‹</button>
          </li>
          {pages.map((p, i) =>
            p === '...' ? (
              <li key={`e${i}`} className="page-item disabled"><span className="page-link">…</span></li>
            ) : (
              <li key={p} className={`page-item ${p === page ? 'active' : ''}`}>
                <button className="page-link" onClick={() => onPage(p)}>{p}</button>
              </li>
            )
          )}
          <li className={`page-item ${page === totalPages ? 'disabled' : ''}`}>
            <button className="page-link" onClick={() => onPage(page + 1)}>›</button>
          </li>
        </ul>
      </nav>
    </div>
  );
}

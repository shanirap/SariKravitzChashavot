export function formatDateDDMMYYYY(value) {
  if (!value) return '—';

  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) return '—';

  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();

  return `${day}/${month}/${year}`;
}

/** For filenames: DD-MM-YYYY (no slashes). */
export function formatDateDDMMYYYYForFilename(value) {
  const formatted = formatDateDDMMYYYY(value);
  if (formatted === '—') return formatted;
  return formatted.replace(/\//g, '-');
}

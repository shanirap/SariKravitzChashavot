/**
 * מחלץ הודעת שגיאה מתגובת axios (JSON רגיל או blob מדוחות).
 * @param {import('axios').AxiosError|{ response?: { data?: unknown } }} err
 * @param {string} fallback
 * @returns {Promise<string>}
 */
export async function parseApiErrorMessage(err, fallback) {
  const res = err?.response;
  if (!res) {
    return 'לא ניתן להתחבר לשרת. ודאו שה־API רץ ושהדפדפן מאשר תעודת SSL מקומית.';
  }

  const d = res.data;
  if (d instanceof Blob) {
    try {
      const t = await d.text();
      const j = JSON.parse(t);
      if (typeof j.message === 'string') return j.message;
      if (typeof j.detail === 'string') return j.detail;
      if (typeof j.title === 'string') return j.title;
    } catch {
      /* non-JSON error body */
    }
    return fallback;
  }

  if (typeof d?.message === 'string') return d.message;
  return fallback;
}

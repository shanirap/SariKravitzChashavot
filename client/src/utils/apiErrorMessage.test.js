import { describe, it, expect } from 'vitest';
import { parseApiErrorMessage } from './apiErrorMessage.js';

describe('parseApiErrorMessage', () => {
  it('returns connection message when no response', async () => {
    const msg = await parseApiErrorMessage({}, 'שגיאה');
    expect(msg).toContain('לא ניתן להתחבר');
  });

  it('reads message from JSON object body', async () => {
    const msg = await parseApiErrorMessage(
      { response: { data: { message: 'employerId נדרש.' } } },
      'שגיאה',
    );
    expect(msg).toBe('employerId נדרש.');
  });

  it('reads message from Blob JSON body (report errors)', async () => {
    const blob = new Blob(
      [JSON.stringify({ message: 'לא נמצאו שורות נתונים בקובץ' })],
      { type: 'application/json' },
    );
    const msg = await parseApiErrorMessage(
      { response: { data: blob } },
      'שגיאה בהעלאה',
    );
    expect(msg).toBe('לא נמצאו שורות נתונים בקובץ');
  });

  it('falls back when blob is not JSON', async () => {
    const blob = new Blob(['not json'], { type: 'text/plain' });
    const msg = await parseApiErrorMessage(
      { response: { data: blob } },
      'שגיאה בהפקת הדוח.',
    );
    expect(msg).toBe('שגיאה בהפקת הדוח.');
  });

  it('prefers detail then title in blob JSON', async () => {
    const blob = new Blob(
      [JSON.stringify({ detail: 'פרט', title: 'כותרת' })],
      { type: 'application/json' },
    );
    const msg = await parseApiErrorMessage({ response: { data: blob } }, 'x');
    expect(msg).toBe('פרט');
  });

  it('blob JSON prefers message over detail and title', async () => {
    const blob = new Blob(
      [JSON.stringify({ message: 'הודעה', detail: 'פרט', title: 'כותרת' })],
      { type: 'application/json' },
    );
    const msg = await parseApiErrorMessage({ response: { data: blob } }, 'x');
    expect(msg).toBe('הודעה');
  });

  it('blob JSON with only title returns title', async () => {
    const blob = new Blob(
      [JSON.stringify({ title: 'כותרת בלבד' })],
      { type: 'application/json' },
    );
    const msg = await parseApiErrorMessage({ response: { data: blob } }, 'ברירת מחדל');
    expect(msg).toBe('כותרת בלבד');
  });

  it('null error without response returns connection message', async () => {
    const msg = await parseApiErrorMessage(null, 'שגיאה כללית');
    expect(msg).toContain('לא ניתן להתחבר');
  });
});

/** ערכי סוג מוסד — תואם ל־InstitutionTypes בשרת */
export const INSTITUTION_TYPE_OPTIONS = ['בית ספר', 'גן', 'אחר'];
export const INSTITUTION_TYPE_DEFAULT = 'אחר';

export function normalizeInstitutionType(value) {
  if (value == null || value === '') return INSTITUTION_TYPE_DEFAULT;
  return INSTITUTION_TYPE_OPTIONS.includes(value) ? value : INSTITUTION_TYPE_DEFAULT;
}

import { describe, it, expect } from 'vitest';
import {
  INSTITUTION_TYPE_OPTIONS,
  INSTITUTION_TYPE_DEFAULT,
  normalizeInstitutionType,
} from './institutionTypes.js';

describe('institutionTypes', () => {
  it('normalizeInstitutionType returns known values unchanged', () => {
    expect(normalizeInstitutionType('גן')).toBe('גן');
    expect(normalizeInstitutionType('בית ספר')).toBe('בית ספר');
  });

  it('normalizeInstitutionType falls back to default', () => {
    expect(normalizeInstitutionType(null)).toBe(INSTITUTION_TYPE_DEFAULT);
    expect(normalizeInstitutionType('unknown')).toBe(INSTITUTION_TYPE_DEFAULT);
  });

  it('options include school kindergarten and other', () => {
    expect(INSTITUTION_TYPE_OPTIONS).toEqual(['בית ספר', 'גן', 'אחר']);
  });

  it('normalizeInstitutionType returns default for empty string', () => {
    expect(normalizeInstitutionType('')).toBe(INSTITUTION_TYPE_DEFAULT);
  });

  it('normalizeInstitutionType keeps explicit אחר', () => {
    expect(normalizeInstitutionType('אחר')).toBe('אחר');
  });

  it('normalizeInstitutionType returns default for whitespace-only unknown', () => {
    expect(normalizeInstitutionType('   ')).toBe(INSTITUTION_TYPE_DEFAULT);
  });
});

import { Temporal } from '@js-temporal/polyfill';

/**
 * Converts a Temporal.PlainDate to a standard JavaScript Date object at local midnight.
 */
export function temporalToDate(plainDate?: Temporal.PlainDate | null): Date | undefined {
  if (!plainDate) return undefined;
  return new Date(plainDate.year, plainDate.month - 1, plainDate.day);
}

/**
 * Converts a standard JavaScript Date object to a Temporal.PlainDate.
 */
export function dateToTemporal(date?: Date | null): Temporal.PlainDate | undefined {
  if (!date) return undefined;
  return Temporal.PlainDate.from({
    year: date.getFullYear(),
    month: date.getMonth() + 1,
    day: date.getDate()
  });
}

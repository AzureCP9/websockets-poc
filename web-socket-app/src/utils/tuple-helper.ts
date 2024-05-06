/**
 * `createTupleFromObjectValues` extracts the values from a given enum-like object and returns them
 * in a tuple format.
 *
 * This helper is useful when working with libraries or utilities that expect a non-empty tuple
 * of string values, particularly when those utilities cannot accept a simple string array. Zod's enum
 * method is a good example.
 *
 * @template T - The type of the enum-like object. It should be a record of string keys to string values.
 *
 * @param {T} obj - The enum-like object from which values should be extracted.
 *
 * @returns {[T[keyof T], ...T[keyof T][]]} - A tuple containing all the values from the given object.
 * The tuple is guaranteed to have at least one value.
 *
 * @example
 * const Colors = { RED: 'Red', GREEN: 'Green', BLUE: 'Blue' } as const;
 * const colorValues = extractValuesAsTuple(Colors); // ['Red', 'Green', 'Blue']
 *
 * @throws {Error} - Throws an error if the provided object is empty.
 **/

function createTupleFromObjectValues<T extends Record<string, string>>(
	obj: T
): [T[keyof T], ...T[keyof T][]] {
	const keys = Object.keys(obj) as (keyof T)[];
	if (keys[0] === undefined) {
		throw new Error("Cannot access keys of an empty object.");
	}
	const values: [T[keyof T], ...T[keyof T][]] = [
		obj[keys[0]],
		...keys.slice(1).map((key) => obj[key]),
	];
	return values;
}

export { createTupleFromObjectValues };

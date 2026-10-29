import type { components } from './schema'

export type Schema<Ad extends keyof components['schemas']> = components['schemas'][Ad]

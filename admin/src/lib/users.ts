import { writable } from 'svelte/store';
import type { User } from './utils';

export const users = writable([] as User[]);

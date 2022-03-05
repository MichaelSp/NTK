import {User} from "$lib/db";

export async function get(): Promise<{ body: any }> {
  return {
    body: {
      users: await User.findAll(),
      errors: []
    }
  };
}
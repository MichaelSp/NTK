import { User }from "$lib/db";
import type { RequestHandler } from "@sveltejs/kit";

export async function get(): Promise<RequestHandler> {
  const users = await User.findAll()

  return {
    body: { users }
  };
}
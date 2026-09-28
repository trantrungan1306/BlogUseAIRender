import Link from "next/link";
import { Container } from "@/components/Container";

export function Footer() {
  return (
    <footer className="mt-20 border-t border-slate-200 bg-white">
      <Container className="flex flex-col items-center justify-between gap-4 py-8 sm:flex-row">
        <div className="flex items-center gap-2">
          <span className="grid h-8 w-8 place-items-center rounded-lg bg-brand-600 text-sm font-bold text-white">S</span>
          <span className="text-sm font-semibold text-slate-900">SimpleBlog</span>
        </div>
        <p className="text-sm text-slate-500">© {new Date().getFullYear()} SimpleBlog. Write. Share. Inspire.</p>
        <div className="flex gap-6 text-sm text-slate-500">
          <Link href="/" className="hover:text-slate-900">Home</Link>
          <Link href="/login" className="hover:text-slate-900">Login</Link>
          <Link href="/register" className="hover:text-slate-900">Join</Link>
        </div>
      </Container>
    </footer>
  );
}

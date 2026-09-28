"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";

interface GoogleCredentialResponse {
  credential?: string;
}

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: {
            client_id: string;
            callback: (response: GoogleCredentialResponse) => void;
          }) => void;
          renderButton: (parent: HTMLElement, options: Record<string, unknown>) => void;
        };
      };
    };
  }
}

const GSI_SRC = "https://accounts.google.com/gsi/client";

// Renders the official Google button when NEXT_PUBLIC_GOOGLE_CLIENT_ID is set;
// otherwise shows a disabled placeholder so the UI still matches the design.
export function GoogleSignInButton({ redirectTo = "/dashboard" }: { redirectTo?: string }) {
  const clientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;
  const { loginWithGoogle } = useAuth();
  const router = useRouter();
  const containerRef = useRef<HTMLDivElement>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!clientId || !containerRef.current) return;

    function init() {
      if (!window.google || !containerRef.current) return;
      window.google.accounts.id.initialize({
        client_id: clientId!,
        callback: async (response) => {
          if (!response.credential) return;
          try {
            await loginWithGoogle(response.credential);
            router.push(redirectTo);
          } catch {
            setError("Google sign-in failed.");
          }
        }
      });
      window.google.accounts.id.renderButton(containerRef.current, {
        theme: "outline",
        size: "large",
        width: 320,
        text: "continue_with"
      });
    }

    if (window.google) {
      init();
      return;
    }

    const existing = document.querySelector(`script[src="${GSI_SRC}"]`);
    if (existing) {
      existing.addEventListener("load", init);
      return () => existing.removeEventListener("load", init);
    }

    const script = document.createElement("script");
    script.src = GSI_SRC;
    script.async = true;
    script.defer = true;
    script.onload = init;
    document.body.appendChild(script);
  }, [clientId, loginWithGoogle, redirectTo, router]);

  if (!clientId) {
    return (
      <button
        type="button"
        disabled
        title="Set NEXT_PUBLIC_GOOGLE_CLIENT_ID to enable Google sign-in"
        className="flex w-full items-center justify-center gap-2 rounded-lg border border-slate-300 px-4 py-2.5 text-sm font-medium text-slate-700 transition hover:bg-slate-50 disabled:opacity-60"
      >
        <svg width="18" height="18" viewBox="0 0 24 24">
          <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92a5.06 5.06 0 0 1-2.2 3.32v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.1Z" />
          <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84A11 11 0 0 0 12 23Z" />
          <path fill="#FBBC05" d="M5.84 14.1a6.6 6.6 0 0 1 0-4.2V7.06H2.18a11 11 0 0 0 0 9.88l3.66-2.84Z" />
          <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1A11 11 0 0 0 2.18 7.06l3.66 2.84C6.71 7.3 9.14 5.38 12 5.38Z" />
        </svg>
        Continue with Google
      </button>
    );
  }

  return (
    <div>
      <div ref={containerRef} className="flex justify-center" />
      {error && <p className="mt-2 text-center text-sm text-rose-600">{error}</p>}
    </div>
  );
}

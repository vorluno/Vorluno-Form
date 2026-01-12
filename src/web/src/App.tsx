// ClientApp/src/App.tsx
import ContactoForm from "./components/ContactoForm";
import { ToastProvider } from "./components/UI/Toast";

export default function App() {
  return (
    <main className="max-w-4xl mx-auto my-6 px-4">
      <header className="mb-6 text-center">
        <img src="/email-assets/vorluno-logo.png" alt="Vorluno" className="h-16 mx-auto mb-4" />
        <h1 className="text-3xl font-bold text-white/90 mb-2">Cuéntanos sobre tu proyecto</h1>
        <p className="text-white/70">Transformamos ideas en software excepcional</p>
      </header>

      <ToastProvider>
        <ContactoForm />
      </ToastProvider>

      <footer className="mt-12 text-center text-white/50 text-sm">
        <p>© 2026 Vorluno. Transformando ideas en realidad.</p>
      </footer>
    </main>
  );
}

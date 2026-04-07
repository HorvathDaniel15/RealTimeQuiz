import { useMemo, useState } from "react";
import OwnerSetupPage from "./pages/OwnerSetupPage";
import AdminQuizListPage from "./pages/AdminQuizListPage";

export default function App() {
  const hasOwner = useMemo(() => !!localStorage.getItem("ownerId"), []);
  const [ready, setReady] = useState(hasOwner);

  if (!ready) return <OwnerSetupPage onSaved={() => setReady(true)} />;

  return (
      <div style={{ padding: 16 }}>
        <h1>RealTimeQuiz Admin MVP</h1>
        <AdminQuizListPage />
      </div>
  );
}

import { Outlet, Link, useLocation } from "react-router-dom";
import { useAuth } from "../App";
import { LayoutDashboard, Users, ArrowLeftRight, LogOut } from "lucide-react";

export default function Layout() {
  const { fullName, logout } = useAuth();
  const location = useLocation();

  const links = [
    { to: "/", label: "Dashboard", icon: LayoutDashboard },
    { to: "/customers", label: "Müşteriler", icon: Users },
    { to: "/transactions", label: "Gelir-Gider", icon: ArrowLeftRight },
  ];

  return (
    <div className="min-h-screen bg-gray-50 flex">
      {/* Sidebar */}
      <aside className="w-64 bg-white border-r border-gray-200 flex flex-col">
        <div className="p-6 border-b border-gray-200">
          <h1 className="text-xl font-bold text-blue-600">KobiPanel</h1>
          <p className="text-sm text-gray-500 mt-1">{fullName}</p>
        </div>

        <nav className="flex-1 p-4 space-y-1">
          {links.map((link) => {
            const isActive = location.pathname === link.to;
            const Icon = link.icon;
            return (
              <Link
                key={link.to}
                to={link.to}
                className={`flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition ${
                  isActive
                    ? "bg-blue-50 text-blue-700"
                    : "text-gray-600 hover:bg-gray-100"
                }`}
              >
                <Icon size={20} />
                {link.label}
              </Link>
            );
          })}
        </nav>

        <div className="p-4 border-t border-gray-200">
          <button
            onClick={logout}
            className="flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium text-red-600 hover:bg-red-50 w-full transition"
          >
            <LogOut size={20} />
            Çıkış Yap
          </button>
        </div>
      </aside>

      {/* Main Content */}
      <main className="flex-1 p-8">
        <Outlet />
      </main>
    </div>
  );
}
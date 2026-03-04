import { useState, useEffect } from "react";
import { useAuth } from "../App";
import api from "../services/api";
import { TrendingUp, TrendingDown, Users, Calendar } from "lucide-react";

export default function Dashboard() {
  const { businessId, setBusinessId } = useAuth();
  const [data, setData] = useState<any>(null);
  const [businesses, setBusinesses] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadBusinesses();
  }, []);

  useEffect(() => {
    if (businessId) loadDashboard();
  }, [businessId]);

  const loadBusinesses = async () => {
    try {
      const res = await api.get("/Businesses");
      setBusinesses(res.data);
      if (res.data.length > 0 && !businessId) {
        setBusinessId(res.data[0].id);
      }
      setLoading(false);
    } catch {
      setLoading(false);
    }
  };

  const loadDashboard = async () => {
    try {
      const res = await api.get(`/businesses/${businessId}/Dashboard`);
      setData(res.data);
    } catch {}
  };

  if (loading) return <div className="text-gray-500">Yükleniyor...</div>;

  if (businesses.length === 0) {
    return (
      <div className="text-center py-20">
        <h2 className="text-2xl font-bold text-gray-800 mb-2">Hoş geldiniz!</h2>
        <p className="text-gray-500 mb-6">Henüz bir işletmeniz yok. Swagger üzerinden işletme ekleyebilirsiniz.</p>
      </div>
    );
  }

  const cards = data ? [
    { label: "Bugün Gelir", value: `₺${data.todayIncome.toFixed(2)}`, icon: TrendingUp, color: "text-green-600", bg: "bg-green-50" },
    { label: "Bugün Gider", value: `₺${data.todayExpense.toFixed(2)}`, icon: TrendingDown, color: "text-red-600", bg: "bg-red-50" },
    { label: "Toplam Müşteri", value: data.totalCustomers, icon: Users, color: "text-blue-600", bg: "bg-blue-50" },
    { label: "Aylık Net Kâr", value: `₺${data.monthlyNetProfit.toFixed(2)}`, icon: TrendingUp, color: "text-purple-600", bg: "bg-purple-50" },
  ] : [];

  return (
    <div>
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-2xl font-bold text-gray-800">Dashboard</h1>
        {businesses.length > 1 && (
          <select
            value={businessId || ""}
            onChange={(e) => setBusinessId(Number(e.target.value))}
            className="border border-gray-300 rounded-lg px-4 py-2 text-sm"
          >
            {businesses.map((b: any) => (
              <option key={b.id} value={b.id}>{b.name}</option>
            ))}
          </select>
        )}
      </div>

      {data && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {cards.map((card, i) => {
            const Icon = card.icon;
            return (
              <div key={i} className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
                <div className="flex items-center justify-between mb-4">
                  <span className="text-sm text-gray-500">{card.label}</span>
                  <div className={`${card.bg} p-2 rounded-lg`}>
                    <Icon size={20} className={card.color} />
                  </div>
                </div>
                <p className={`text-2xl font-bold ${card.color}`}>{card.value}</p>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
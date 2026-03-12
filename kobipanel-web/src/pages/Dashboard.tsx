import { useState, useEffect } from "react";
import { useAuth } from "../App";
import api from "../services/api";
import { TrendingUp, TrendingDown, Users, Clock, Plus, X } from "lucide-react";
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend, PieChart, Pie, Cell } from "recharts";

const COLORS = ["#3b82f6", "#ef4444", "#22c55e", "#f59e0b", "#8b5cf6", "#ec4899"];

export default function Dashboard() {
  const { businessId, setBusinessId } = useAuth();
  const [data, setData] = useState<any>(null);
  const [chartData, setChartData] = useState<any[]>([]);
  const [businesses, setBusinesses] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAddBusiness, setShowAddBusiness] = useState(false);
  const [businessForm, setBusinessForm] = useState({ name: "", description: "", address: "", phoneNumber: "", email: "", taxNumber: "", businessType: "General" });
  const [formLoading, setFormLoading] = useState(false);

  useEffect(() => { loadBusinesses(); }, []);
  useEffect(() => { if (businessId) { loadDashboard(); loadChart(); } }, [businessId]);

  const loadBusinesses = async () => {
    try {
      const res = await api.get("/Businesses");
      setBusinesses(res.data);
      if (res.data.length > 0 && !businessId) setBusinessId(res.data[0].id);
      setLoading(false);
    } catch { setLoading(false); }
  };

  const loadDashboard = async () => {
    try { const res = await api.get(`/businesses/${businessId}/Dashboard`); setData(res.data); } catch {}
  };

  const loadChart = async () => {
    try { const res = await api.get(`/businesses/${businessId}/Transactions/chart?days=30`); setChartData(res.data); } catch {}
  };

  const handleAddBusiness = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormLoading(true);
    try {
      const res = await api.post("/Businesses", businessForm);
      setBusinessForm({ name: "", description: "", address: "", phoneNumber: "", email: "", taxNumber: "", businessType: "General" });
      setShowAddBusiness(false);
      await loadBusinesses();
      setBusinessId(res.data.id);
    } catch {}
    setFormLoading(false);
  };

  if (loading) return <div className="flex items-center justify-center h-64 text-gray-500">Yükleniyor...</div>;

  if (businesses.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-64">
        <h2 className="text-2xl font-bold text-gray-800 mb-2">Hoş geldiniz!</h2>
        <p className="text-gray-500 mb-6">Başlamak için ilk işletmenizi oluşturun.</p>
        <button onClick={() => setShowAddBusiness(true)} className="flex items-center gap-2 bg-blue-600 text-white px-6 py-3 rounded-lg font-medium hover:bg-blue-700 transition">
          <Plus size={20} /> İşletme Oluştur
        </button>
        {renderBusinessModal()}
      </div>
    );
  }

  function renderBusinessModal() {
    if (!showAddBusiness) return null;
    return (
      <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
        <div className="bg-white rounded-2xl p-6 w-full max-w-md">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-bold text-gray-800">Yeni İşletme</h2>
            <button onClick={() => setShowAddBusiness(false)}><X size={20} className="text-gray-400" /></button>
          </div>
          <form onSubmit={handleAddBusiness}>
            <input placeholder="İşletme Adı *" value={businessForm.name} onChange={(e) => setBusinessForm({ ...businessForm, name: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 mb-3" required />
            <input placeholder="Açıklama" value={businessForm.description} onChange={(e) => setBusinessForm({ ...businessForm, description: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 mb-3" />
            <input placeholder="Adres" value={businessForm.address} onChange={(e) => setBusinessForm({ ...businessForm, address: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 mb-3" />
            <div className="grid grid-cols-2 gap-3 mb-3">
              <input placeholder="Telefon" value={businessForm.phoneNumber} onChange={(e) => setBusinessForm({ ...businessForm, phoneNumber: e.target.value })} className="px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500" />
              <input placeholder="Email" value={businessForm.email} onChange={(e) => setBusinessForm({ ...businessForm, email: e.target.value })} className="px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div className="grid grid-cols-2 gap-3 mb-4">
              <input placeholder="Vergi No" value={businessForm.taxNumber} onChange={(e) => setBusinessForm({ ...businessForm, taxNumber: e.target.value })} className="px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500" />
              <select value={businessForm.businessType} onChange={(e) => setBusinessForm({ ...businessForm, businessType: e.target.value })} className="px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500">
                <option value="General">Genel</option>
                <option value="Berber">Berber</option>
                <option value="Market">Market</option>
                <option value="Klinik">Klinik</option>
                <option value="Restoran">Restoran</option>
                <option value="OtoServis">Oto Servis</option>
                <option value="Butik">Butik</option>
                <option value="Diger">Diğer</option>
              </select>
            </div>
            <button type="submit" disabled={formLoading} className="w-full bg-blue-600 text-white py-3 rounded-lg font-medium hover:bg-blue-700 transition disabled:opacity-50">
              {formLoading ? "Oluşturuluyor..." : "İşletme Oluştur"}
            </button>
          </form>
        </div>
      </div>
    );
  }

  const cards = data ? [
    { label: "Bugün Gelir", value: `₺${data.todayIncome.toFixed(2)}`, icon: TrendingUp, color: "text-green-600", bg: "bg-green-50", border: "border-green-100" },
    { label: "Bugün Gider", value: `₺${data.todayExpense.toFixed(2)}`, icon: TrendingDown, color: "text-red-600", bg: "bg-red-50", border: "border-red-100" },
    { label: "Toplam Müşteri", value: data.totalCustomers, icon: Users, color: "text-blue-600", bg: "bg-blue-50", border: "border-blue-100" },
    { label: "Aylık Net Kâr", value: `₺${data.monthlyNetProfit.toFixed(2)}`, icon: TrendingUp, color: "text-purple-600", bg: "bg-purple-50", border: "border-purple-100" },
  ] : [];

  const expenseData = data?.expenseByCategory?.map((item: any, i: number) => ({
    ...item, color: COLORS[i % COLORS.length]
  })) || [];

  return (
    <div>
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between mb-8 gap-4">
        <h1 className="text-2xl font-bold text-gray-800">Dashboard</h1>
        <div className="flex items-center gap-3">
          {businesses.length > 0 && (
            <select value={businessId || ""} onChange={(e) => setBusinessId(Number(e.target.value))} className="border border-gray-300 rounded-lg px-4 py-2 text-sm">
              {businesses.map((b: any) => (<option key={b.id} value={b.id}>{b.name}</option>))}
            </select>
          )}
          <button onClick={() => setShowAddBusiness(true)} className="flex items-center gap-1 text-blue-600 text-sm font-medium hover:underline">
            <Plus size={16} /> Yeni İşletme
          </button>
        </div>
      </div>

      {/* KPI Kartları */}
      {data && (
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6 mb-8">
          {cards.map((card, i) => {
            const Icon = card.icon;
            return (
              <div key={i} className={`bg-white rounded-xl p-4 sm:p-6 shadow-sm border ${card.border}`}>
                <div className="flex items-center justify-between mb-3">
                  <span className="text-xs sm:text-sm text-gray-500">{card.label}</span>
                  <div className={`${card.bg} p-2 rounded-lg hidden sm:block`}><Icon size={20} className={card.color} /></div>
                </div>
                <p className={`text-lg sm:text-2xl font-bold ${card.color}`}>{card.value}</p>
              </div>
            );
          })}
        </div>
      )}

      {/* Grafikler */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
        {/* Gelir-Gider Grafiği */}
        {chartData.length > 0 && (
          <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-gray-100 p-4 sm:p-6">
            <h2 className="text-lg font-semibold text-gray-800 mb-4">Son 30 Gün</h2>
            <ResponsiveContainer width="100%" height={280}>
              <AreaChart data={chartData}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                <XAxis dataKey="date" tick={{ fontSize: 10 }} stroke="#9ca3af" />
                <YAxis tick={{ fontSize: 10 }} stroke="#9ca3af" />
                <Tooltip formatter={(value: any) => `₺${Number(value).toFixed(2)}`} />
                <Legend />
                <Area type="monotone" dataKey="gelir" name="Gelir" stroke="#22c55e" fill="#dcfce7" strokeWidth={2} />
                <Area type="monotone" dataKey="gider" name="Gider" stroke="#ef4444" fill="#fee2e2" strokeWidth={2} />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        )}

        {/* Gider Dağılımı Pasta Grafiği */}
        {expenseData.length > 0 && (
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4 sm:p-6">
            <h2 className="text-lg font-semibold text-gray-800 mb-4">Gider Dağılımı</h2>
            <ResponsiveContainer width="100%" height={220}>
              <PieChart>
                <Pie data={expenseData} dataKey="amount" nameKey="categoryName" cx="50%" cy="50%" outerRadius={80} label={({ categoryName, percent }: any) => `${categoryName} ${(percent * 100).toFixed(0)}%`}>
                  {expenseData.map((entry: any, index: number) => (
                    <Cell key={index} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip formatter={(value: any) => `₺${Number(value).toFixed(2)}`} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        )}
      </div>

      {/* Yaklaşan Randevular */}
      {data?.upcomingAppointments?.length > 0 && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4 sm:p-6">
          <h2 className="text-lg font-semibold text-gray-800 mb-4">Yaklaşan Randevular</h2>
          <div className="space-y-3">
            {data.upcomingAppointments.map((apt: any) => (
              <div key={apt.id} className="flex items-center justify-between py-2 border-b border-gray-50 last:border-0">
                <div>
                  <p className="text-sm font-medium text-gray-800">{apt.title}</p>
                  <p className="text-xs text-gray-500">{apt.customerName || "Müşteri belirtilmemiş"}</p>
                </div>
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <Clock size={14} />
                  {new Date(apt.startTime).toLocaleDateString("tr-TR")} {new Date(apt.startTime).toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" })}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {renderBusinessModal()}
    </div>
  );
}
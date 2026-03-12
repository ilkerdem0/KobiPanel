import { useState, useEffect } from "react";
import { useAuth } from "../App";
import api from "../services/api";
import { Plus, X, Check, Clock, AlertTriangle } from "lucide-react";
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from "recharts";

export default function Transactions() {
  const { businessId } = useAuth();
  const [transactions, setTransactions] = useState<any>({ items: [], totalCount: 0 });
  const [summary, setSummary] = useState<any>(null);
  const [chartData, setChartData] = useState<any[]>([]);
  const [filter, setFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({
    description: "", amount: "", type: "Income",
    transactionDate: new Date().toISOString().split("T")[0],
    paymentMethod: "Cash", categoryId: null, customerId: null,
    isScheduled: false, dueDate: ""
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (businessId) {
      loadTransactions();
      loadSummary();
      loadChart();
    }
  }, [businessId, filter, statusFilter]);

  const loadTransactions = async () => {
    try {
      let params = `?pageSize=50`;
      if (filter) params += `&type=${filter}`;
      if (statusFilter) params += `&paymentStatus=${statusFilter}`;
      const res = await api.get(`/businesses/${businessId}/Transactions${params}`);
      setTransactions(res.data);
    } catch {}
  };

  const loadSummary = async () => {
    try {
      const now = new Date();
      const from = new Date(now.getFullYear(), now.getMonth(), 1).toISOString();
      const to = now.toISOString();
      const res = await api.get(`/businesses/${businessId}/Transactions/summary?from=${from}&to=${to}`);
      setSummary(res.data);
    } catch {}
  };

  const loadChart = async () => {
    try {
      const res = await api.get(`/businesses/${businessId}/Transactions/chart?days=30`);
      setChartData(res.data);
    } catch {}
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await api.post(`/businesses/${businessId}/Transactions`, {
        ...form,
        amount: parseFloat(form.amount),
        transactionDate: new Date(form.transactionDate).toISOString(),
        dueDate: form.isScheduled && form.dueDate ? new Date(form.dueDate).toISOString() : null
      });
      setForm({ description: "", amount: "", type: "Income", transactionDate: new Date().toISOString().split("T")[0], paymentMethod: "Cash", categoryId: null, customerId: null, isScheduled: false, dueDate: "" });
      setShowForm(false);
      loadTransactions();
      loadSummary();
      loadChart();
    } catch {}
    setLoading(false);
  };

  const handleComplete = async (id: number) => {
    await api.put(`/businesses/${businessId}/Transactions/${id}/complete`);
    loadTransactions();
    loadSummary();
    loadChart();
  };

  const handleDelete = async (id: number) => {
    if (window.confirm("Bu işlemi silmek istediğinize emin misiniz?")) {
      await api.delete(`/businesses/${businessId}/Transactions/${id}`);
      loadTransactions();
      loadSummary();
      loadChart();
    }
  };

  if (!businessId) return <div className="text-gray-500">Önce bir işletme seçin.</div>;

  const statusBadge = (status: string) => {
    if (status === "Completed") return <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-700"><Check size={12} /> Tamamlandı</span>;
    if (status === "Pending") return <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-700"><Clock size={12} /> Bekliyor</span>;
    if (status === "Overdue") return <span className="inline-flex items-center gap-1 px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-700"><AlertTriangle size={12} /> Gecikmiş</span>;
    return null;
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Gelir - Gider</h1>
        <button onClick={() => setShowForm(true)} className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 transition">
          <Plus size={18} /> Yeni İşlem
        </button>
      </div>

      {/* Özet Kartları */}
      {summary && (
        <div className="grid grid-cols-2 lg:grid-cols-5 gap-4 mb-6">
          <div className="bg-green-50 rounded-xl p-4 border border-green-100">
            <p className="text-xs text-green-600 font-medium">Aylık Gelir</p>
            <p className="text-xl font-bold text-green-700">₺{summary.totalIncome.toFixed(2)}</p>
          </div>
          <div className="bg-red-50 rounded-xl p-4 border border-red-100">
            <p className="text-xs text-red-600 font-medium">Aylık Gider</p>
            <p className="text-xl font-bold text-red-700">₺{summary.totalExpense.toFixed(2)}</p>
          </div>
          <div className="bg-blue-50 rounded-xl p-4 border border-blue-100">
            <p className="text-xs text-blue-600 font-medium">Net Kâr</p>
            <p className="text-xl font-bold text-blue-700">₺{summary.netProfit.toFixed(2)}</p>
          </div>
          <div className="bg-yellow-50 rounded-xl p-4 border border-yellow-100">
            <p className="text-xs text-yellow-600 font-medium">Bekleyen Gelir</p>
            <p className="text-xl font-bold text-yellow-700">₺{summary.pendingIncome.toFixed(2)}</p>
          </div>
          <div className="bg-orange-50 rounded-xl p-4 border border-orange-100">
            <p className="text-xs text-orange-600 font-medium">Gecikmiş</p>
            <p className="text-xl font-bold text-orange-700">₺{(summary.overdueIncome + summary.overdueExpense).toFixed(2)}</p>
          </div>
        </div>
      )}

      {/* Grafik */}
      {chartData.length > 0 && (
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-6">
          <h2 className="text-sm font-medium text-gray-500 mb-4">Son 30 Gün Gelir-Gider</h2>
          <ResponsiveContainer width="100%" height={250}>
            <AreaChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
              <XAxis dataKey="date" tick={{ fontSize: 11 }} stroke="#9ca3af" />
              <YAxis tick={{ fontSize: 11 }} stroke="#9ca3af" />
              <Tooltip formatter={(value: any) => `₺${Number(value).toFixed(2)}`} />              <Legend />
              <Area type="monotone" dataKey="gelir" name="Gelir" stroke="#22c55e" fill="#dcfce7" strokeWidth={2} />
              <Area type="monotone" dataKey="gider" name="Gider" stroke="#ef4444" fill="#fee2e2" strokeWidth={2} />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Filtreler */}
      <div className="flex gap-2 mb-4 flex-wrap">
        <div className="flex gap-2">
          {[{ v: "", l: "Tümü" }, { v: "Income", l: "Gelirler" }, { v: "Expense", l: "Giderler" }].map((f) => (
            <button key={f.v} onClick={() => setFilter(f.v)}
              className={`px-4 py-2 rounded-lg text-sm font-medium transition ${filter === f.v ? "bg-blue-600 text-white" : "bg-white text-gray-600 border border-gray-300 hover:bg-gray-50"}`}>
              {f.l}
            </button>
          ))}
        </div>
        <div className="flex gap-2 ml-4">
          {[{ v: "", l: "Tüm Durum" }, { v: "Completed", l: "Tamamlanan" }, { v: "Pending", l: "Bekleyen" }, { v: "Overdue", l: "Gecikmiş" }].map((f) => (
            <button key={f.v} onClick={() => setStatusFilter(f.v)}
              className={`px-3 py-2 rounded-lg text-xs font-medium transition ${statusFilter === f.v ? "bg-gray-800 text-white" : "bg-white text-gray-600 border border-gray-300 hover:bg-gray-50"}`}>
              {f.l}
            </button>
          ))}
        </div>
      </div>

      {/* İşlem Listesi */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        {transactions.items?.length === 0 ? (
          <div className="p-8 text-center text-gray-500">Henüz işlem eklenmemiş.</div>
        ) : (
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Açıklama</th>
                <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Tarih</th>
                <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Vade</th>
                <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Durum</th>
                <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Ödeme</th>
                <th className="text-right px-6 py-3 text-sm font-medium text-gray-500">Tutar</th>
                <th className="text-right px-6 py-3 text-sm font-medium text-gray-500">İşlem</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {transactions.items?.map((t: any) => (
                <tr key={t.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm text-gray-800">{t.description}</td>
                  <td className="px-6 py-4 text-sm text-gray-600">{new Date(t.transactionDate).toLocaleDateString("tr-TR")}</td>
                  <td className="px-6 py-4 text-sm text-gray-600">{t.dueDate ? new Date(t.dueDate).toLocaleDateString("tr-TR") : "-"}</td>
                  <td className="px-6 py-4">{statusBadge(t.paymentStatus)}</td>
                  <td className="px-6 py-4 text-sm text-gray-600">{t.paymentMethod || "-"}</td>
                  <td className={`px-6 py-4 text-sm font-medium text-right ${t.type === "Income" ? "text-green-600" : "text-red-600"}`}>
                    {t.type === "Income" ? "+" : "-"}₺{t.amount.toFixed(2)}
                  </td>
                  <td className="px-6 py-4 text-right space-x-2">
                    {t.paymentStatus !== "Completed" && (
                      <button onClick={() => handleComplete(t.id)} className="text-green-500 hover:text-green-700 text-sm">Tahsil Et</button>
                    )}
                    <button onClick={() => handleDelete(t.id)} className="text-red-500 hover:text-red-700 text-sm">Sil</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Yeni İşlem Modal */}
      {showForm && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-2xl p-6 w-full max-w-md max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-bold text-gray-800">Yeni İşlem</h2>
              <button onClick={() => setShowForm(false)}><X size={20} className="text-gray-400" /></button>
            </div>
            <form onSubmit={handleCreate}>
              <input placeholder="Açıklama" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 mb-3" required />
              <input type="number" step="0.01" placeholder="Tutar (₺)" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 mb-3" required />
              <div className="grid grid-cols-2 gap-3 mb-3">
                <select value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value })} className="px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500">
                  <option value="Income">Gelir</option>
                  <option value="Expense">Gider</option>
                </select>
                <select value={form.paymentMethod} onChange={(e) => setForm({ ...form, paymentMethod: e.target.value })} className="px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500">
                  <option value="Cash">Nakit</option>
                  <option value="CreditCard">Kredi Kartı</option>
                  <option value="BankTransfer">Havale/EFT</option>
                </select>
              </div>
              <input type="date" value={form.transactionDate} onChange={(e) => setForm({ ...form, transactionDate: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 mb-3" />

              {/* Vadeli İşlem */}
              <label className="flex items-center gap-2 mb-3 cursor-pointer">
                <input type="checkbox" checked={form.isScheduled} onChange={(e) => setForm({ ...form, isScheduled: e.target.checked })} className="w-4 h-4 text-blue-600 rounded" />
                <span className="text-sm text-gray-700">Vadeli İşlem</span>
              </label>

              {form.isScheduled && (
                <div className="mb-3">
                  <label className="block text-sm text-gray-600 mb-1">Vade Tarihi</label>
                  <input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500" required />
                </div>
              )}

              <button type="submit" disabled={loading} className="w-full bg-blue-600 text-white py-3 rounded-lg font-medium hover:bg-blue-700 transition disabled:opacity-50 mt-2">
                {loading ? "Kaydediliyor..." : "Kaydet"}
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
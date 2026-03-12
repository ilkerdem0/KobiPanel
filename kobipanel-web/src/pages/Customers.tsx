import { useState, useEffect } from "react";
import { useAuth } from "../App";
import api from "../services/api";
import { Plus, Search, X } from "lucide-react";
import { useNavigate } from "react-router-dom";

export default function Customers() {
  const { businessId } = useAuth();
  const navigate = useNavigate();
  const [customers, setCustomers] = useState<any>({ items: [], totalCount: 0 });
  const [search, setSearch] = useState("");
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ firstName: "", lastName: "", phoneNumber: "", email: "", notes: "" });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (businessId) loadCustomers();
  }, [businessId, search]);

  const loadCustomers = async () => {
    try {
      const res = await api.get(`/businesses/${businessId}/Customers?search=${search}`);
      setCustomers(res.data);
    } catch {}
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await api.post(`/businesses/${businessId}/Customers`, form);
      setForm({ firstName: "", lastName: "", phoneNumber: "", email: "", notes: "" });
      setShowForm(false);
      loadCustomers();
    } catch {}
    setLoading(false);
  };

  const handleDelete = async (id: number) => {
    if (window.confirm("Bu müşteriyi silmek istediğinize emin misiniz?")) {
      await api.delete(`/businesses/${businessId}/Customers/${id}`);
      loadCustomers();
    }
  };

  if (!businessId) return <div className="text-gray-500">Önce bir işletme seçin.</div>;

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Müşteriler</h1>
        <button
          onClick={() => setShowForm(true)}
          className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 transition"
        >
          <Plus size={18} /> Yeni Müşteri
        </button>
      </div>

      {/* Arama */}
      <div className="relative mb-6">
        <Search size={18} className="absolute left-3 top-3 text-gray-400" />
        <input
          type="text"
          placeholder="İsim veya telefon ile ara..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
        />
      </div>

      {/* Müşteri Listesi */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        {customers.items?.length === 0 ? (
          <div className="p-8 text-center text-gray-500">Henüz müşteri eklenmemiş.</div>
        ) : (
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Ad Soyad</th>
                <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Telefon</th>
                <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Toplam Harcama</th>
                <th className="text-right px-6 py-3 text-sm font-medium text-gray-500">İşlem</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {customers.items?.map((c: any) => (
                <tr key={c.id} className="hover:bg-gray-50">
                 <td className="px-6 py-4 text-sm font-medium text-blue-600 cursor-pointer hover:underline" onClick={() => navigate(`/customers/${c.id}`)}>{c.fullName}</td>
                  <td className="px-6 py-4 text-sm text-gray-600">{c.phoneNumber || "-"}</td>
                  <td className="px-6 py-4 text-sm text-gray-600">₺{c.totalSpent?.toFixed(2)}</td>
                  <td className="px-6 py-4 text-right">
                    <button onClick={() => handleDelete(c.id)} className="text-red-500 hover:text-red-700 text-sm">Sil</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Yeni Müşteri Modal */}
      {showForm && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-2xl p-6 w-full max-w-md">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-bold text-gray-800">Yeni Müşteri</h2>
              <button onClick={() => setShowForm(false)}><X size={20} className="text-gray-400" /></button>
            </div>
            <form onSubmit={handleCreate}>
              <div className="grid grid-cols-2 gap-3 mb-3">
                <input placeholder="Ad" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} className="px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500" required />
                <input placeholder="Soyad" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} className="px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500" required />
              </div>
              <input placeholder="Telefon" value={form.phoneNumber} onChange={(e) => setForm({ ...form, phoneNumber: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 mb-3" />
              <input placeholder="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 mb-3" />
              <textarea placeholder="Notlar" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 mb-4" rows={2} />
              <button type="submit" disabled={loading} className="w-full bg-blue-600 text-white py-3 rounded-lg font-medium hover:bg-blue-700 transition disabled:opacity-50">
                {loading ? "Kaydediliyor..." : "Kaydet"}
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
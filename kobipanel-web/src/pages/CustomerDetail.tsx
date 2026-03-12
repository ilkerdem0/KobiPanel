import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAuth } from "../App";
import api from "../services/api";
import { ArrowLeft, Phone, Mail, StickyNote } from "lucide-react";

export default function CustomerDetail() {
  const { businessId } = useAuth();
  const { customerId } = useParams();
  const navigate = useNavigate();
  const [customer, setCustomer] = useState<any>(null);
  const [transactions, setTransactions] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (businessId && customerId) {
      loadCustomer();
      loadTransactions();
    }
  }, [businessId, customerId]);

  const loadCustomer = async () => {
    try {
      const res = await api.get(`/businesses/${businessId}/Customers/${customerId}`);
      setCustomer(res.data);
      setLoading(false);
    } catch { setLoading(false); }
  };

  const loadTransactions = async () => {
    try {
      const res = await api.get(`/businesses/${businessId}/Transactions?pageSize=50`);
      const filtered = res.data.items?.filter((t: any) => t.customerName === customer?.fullName) || [];
      setTransactions(res.data.items || []);
    } catch {}
  };

  if (loading) return <div className="text-gray-500">Yükleniyor...</div>;
  if (!customer) return <div className="text-gray-500">Müşteri bulunamadı.</div>;

  return (
    <div>
      <button onClick={() => navigate("/customers")} className="flex items-center gap-2 text-gray-500 hover:text-gray-800 mb-6 text-sm">
        <ArrowLeft size={18} /> Müşterilere Dön
      </button>

      {/* Müşteri Bilgileri */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-6">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-800">{customer.fullName}</h1>
            <div className="flex flex-wrap gap-4 mt-3 text-sm text-gray-600">
              {customer.phoneNumber && (
                <span className="flex items-center gap-1"><Phone size={14} /> {customer.phoneNumber}</span>
              )}
              {customer.email && (
                <span className="flex items-center gap-1"><Mail size={14} /> {customer.email}</span>
              )}
            </div>
            {customer.notes && (
              <p className="flex items-center gap-1 mt-2 text-sm text-gray-500"><StickyNote size={14} /> {customer.notes}</p>
            )}
          </div>
          <div className="flex gap-4">
            <div className="bg-green-50 rounded-xl p-4 text-center min-w-[120px]">
              <p className="text-xs text-green-600 font-medium">Toplam Harcama</p>
              <p className="text-xl font-bold text-green-700">₺{customer.totalSpent?.toFixed(2)}</p>
            </div>
            <div className="bg-blue-50 rounded-xl p-4 text-center min-w-[120px]">
              <p className="text-xs text-blue-600 font-medium">İşlem Sayısı</p>
              <p className="text-xl font-bold text-blue-700">{customer.transactionCount || 0}</p>
            </div>
          </div>
        </div>
      </div>

      {/* İşlem Geçmişi */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="px-6 py-4 border-b border-gray-200">
          <h2 className="text-lg font-semibold text-gray-800">İşlem Geçmişi</h2>
        </div>
        {transactions.filter((t: any) => t.customerName === customer.fullName).length === 0 ? (
          <div className="p-8 text-center text-gray-500">Bu müşteriye ait işlem bulunmuyor.</div>
        ) : (
          <table className="w-full">
            <thead className="bg-gray-50 border-b border-gray-200">
              <tr>
                <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Açıklama</th>
                <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Tarih</th>
                <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Durum</th>
                <th className="text-right px-6 py-3 text-sm font-medium text-gray-500">Tutar</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {transactions.filter((t: any) => t.customerName === customer.fullName).map((t: any) => (
                <tr key={t.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 text-sm text-gray-800">{t.description}</td>
                  <td className="px-6 py-4 text-sm text-gray-600">{new Date(t.transactionDate).toLocaleDateString("tr-TR")}</td>
                  <td className="px-6 py-4 text-sm text-gray-600">{t.paymentStatus}</td>
                  <td className={`px-6 py-4 text-sm font-medium text-right ${t.type === "Income" ? "text-green-600" : "text-red-600"}`}>
                    {t.type === "Income" ? "+" : "-"}₺{t.amount.toFixed(2)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
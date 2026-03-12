import { useState, useEffect } from "react";
import { useAuth } from "../App";
import api from "../services/api";
import { Plus, X, FileText, Trash2 } from "lucide-react";

export default function Invoices() {
  const { businessId } = useAuth();
  const [invoices, setInvoices] = useState<any>({ items: [], totalCount: 0 });
  const [customers, setCustomers] = useState<any[]>([]);
  const [selectedInvoice, setSelectedInvoice] = useState<any>(null);
  const [statusFilter, setStatusFilter] = useState("");
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({
    customerId: "", dueDate: "", taxRate: "20", notes: "",
    items: [{ description: "", quantity: "1", unitPrice: "" }]
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (businessId) {
      loadInvoices();
      loadCustomers();
    }
  }, [businessId, statusFilter]);

  const loadInvoices = async () => {
    try {
      let params = `?pageSize=50`;
      if (statusFilter) params += `&status=${statusFilter}`;
      const res = await api.get(`/businesses/${businessId}/Invoices${params}`);
      setInvoices(res.data);
    } catch {}
  };

  const loadCustomers = async () => {
    try {
      const res = await api.get(`/businesses/${businessId}/Customers?pageSize=100`);
      setCustomers(res.data.items || []);
    } catch {}
  };

  const loadInvoiceDetail = async (id: number) => {
    try {
      const res = await api.get(`/businesses/${businessId}/Invoices/${id}`);
      setSelectedInvoice(res.data);
    } catch {}
  };

  const addItem = () => {
    setForm({ ...form, items: [...form.items, { description: "", quantity: "1", unitPrice: "" }] });
  };

  const removeItem = (index: number) => {
    if (form.items.length === 1) return;
    setForm({ ...form, items: form.items.filter((_, i) => i !== index) });
  };

  const updateItem = (index: number, field: string, value: string) => {
    const newItems = [...form.items];
    (newItems[index] as any)[field] = value;
    setForm({ ...form, items: newItems });
  };

  const getSubTotal = () => {
    return form.items.reduce((sum, item) => sum + (parseFloat(item.quantity) || 0) * (parseFloat(item.unitPrice) || 0), 0);
  };

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await api.post(`/businesses/${businessId}/Invoices`, {
        customerId: form.customerId ? parseInt(form.customerId) : null,
        dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null,
        taxRate: parseFloat(form.taxRate),
        notes: form.notes || null,
        items: form.items.map(item => ({
          description: item.description,
          quantity: parseInt(item.quantity),
          unitPrice: parseFloat(item.unitPrice)
        }))
      });
      setForm({ customerId: "", dueDate: "", taxRate: "20", notes: "", items: [{ description: "", quantity: "1", unitPrice: "" }] });
      setShowForm(false);
      loadInvoices();
    } catch {}
    setLoading(false);
  };

  const handleStatusChange = async (invoiceId: number, status: string) => {
    await api.put(`/businesses/${businessId}/Invoices/${invoiceId}/status`, { status });
    loadInvoices();
    if (selectedInvoice?.id === invoiceId) loadInvoiceDetail(invoiceId);
  };

  const handleDelete = async (id: number) => {
    if (window.confirm("Bu faturayı silmek istediğinize emin misiniz?")) {
      await api.delete(`/businesses/${businessId}/Invoices/${id}`);
      setSelectedInvoice(null);
      loadInvoices();
    }
  };

  if (!businessId) return <div className="text-gray-500">Önce bir işletme seçin.</div>;

  const statusBadge = (status: string) => {
    const styles: any = {
      Draft: "bg-gray-100 text-gray-700",
      Sent: "bg-blue-100 text-blue-700",
      Paid: "bg-green-100 text-green-700",
      Cancelled: "bg-red-100 text-red-700"
    };
    const labels: any = { Draft: "Taslak", Sent: "Gönderildi", Paid: "Ödendi", Cancelled: "İptal" };
    return <span className={`px-2 py-1 rounded-full text-xs font-medium ${styles[status] || ""}`}>{labels[status] || status}</span>;
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Faturalar</h1>
        <button onClick={() => setShowForm(true)} className="flex items-center gap-2 bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 transition">
          <Plus size={18} /> Yeni Fatura
        </button>
      </div>

      {/* Filtreler */}
      <div className="flex gap-2 mb-4">
        {[{ v: "", l: "Tümü" }, { v: "Draft", l: "Taslak" }, { v: "Sent", l: "Gönderildi" }, { v: "Paid", l: "Ödendi" }, { v: "Cancelled", l: "İptal" }].map((f) => (
          <button key={f.v} onClick={() => setStatusFilter(f.v)}
            className={`px-4 py-2 rounded-lg text-sm font-medium transition ${statusFilter === f.v ? "bg-blue-600 text-white" : "bg-white text-gray-600 border border-gray-300 hover:bg-gray-50"}`}>
            {f.l}
          </button>
        ))}
      </div>

      <div className="flex gap-6">
        {/* Fatura Listesi */}
        <div className="flex-1 bg-white rounded-xl shadow-sm border border-gray-100">
          {invoices.items?.length === 0 ? (
            <div className="p-8 text-center text-gray-500">Henüz fatura oluşturulmamış.</div>
          ) : (
            <table className="w-full">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Fatura No</th>
                  <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Müşteri</th>
                  <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Tarih</th>
                  <th className="text-left px-6 py-3 text-sm font-medium text-gray-500">Durum</th>
                  <th className="text-right px-6 py-3 text-sm font-medium text-gray-500">Tutar</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {invoices.items?.map((inv: any) => (
                  <tr key={inv.id} className="hover:bg-gray-50 cursor-pointer" onClick={() => loadInvoiceDetail(inv.id)}>
                    <td className="px-6 py-4 text-sm font-medium text-blue-600">{inv.invoiceNumber}</td>
                    <td className="px-6 py-4 text-sm text-gray-600">{inv.customerName || "-"}</td>
                    <td className="px-6 py-4 text-sm text-gray-600">{new Date(inv.invoiceDate).toLocaleDateString("tr-TR")}</td>
                    <td className="px-6 py-4">{statusBadge(inv.status)}</td>
                    <td className="px-6 py-4 text-sm font-medium text-right text-gray-800">₺{inv.totalAmount.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        {/* Fatura Detay */}
        {selectedInvoice && (
          <div className="w-96 bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-bold text-gray-800">{selectedInvoice.invoiceNumber}</h2>
              <button onClick={() => setSelectedInvoice(null)}><X size={18} className="text-gray-400" /></button>
            </div>

            <div className="space-y-3 mb-6">
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Müşteri</span>
                <span className="text-gray-800 font-medium">{selectedInvoice.customerName || "-"}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Tarih</span>
                <span className="text-gray-800">{new Date(selectedInvoice.invoiceDate).toLocaleDateString("tr-TR")}</span>
              </div>
              {selectedInvoice.dueDate && (
                <div className="flex justify-between text-sm">
                  <span className="text-gray-500">Vade</span>
                  <span className="text-gray-800">{new Date(selectedInvoice.dueDate).toLocaleDateString("tr-TR")}</span>
                </div>
              )}
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Durum</span>
                {statusBadge(selectedInvoice.status)}
              </div>
            </div>

            {/* Kalemler */}
            <div className="border-t border-gray-200 pt-4 mb-4">
              <h3 className="text-sm font-medium text-gray-500 mb-2">Kalemler</h3>
              {selectedInvoice.items?.map((item: any, i: number) => (
                <div key={i} className="flex justify-between text-sm py-1">
                  <span className="text-gray-700">{item.description} x{item.quantity}</span>
                  <span className="text-gray-800">₺{item.totalPrice.toFixed(2)}</span>
                </div>
              ))}
            </div>

            {/* Toplam */}
            <div className="border-t border-gray-200 pt-4 space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Ara Toplam</span>
                <span>₺{selectedInvoice.subTotal.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">KDV (%{selectedInvoice.taxRate})</span>
                <span>₺{selectedInvoice.taxAmount.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-base font-bold">
                <span>Toplam</span>
                <span className="text-blue-600">₺{selectedInvoice.totalAmount.toFixed(2)}</span>
              </div>
            </div>

            {/* Durum Butonları */}
            <div className="mt-6 space-y-2">
              {selectedInvoice.status === "Draft" && (
                <button onClick={() => handleStatusChange(selectedInvoice.id, "Sent")} className="w-full bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700 transition">Gönderildi Olarak İşaretle</button>
              )}
              {selectedInvoice.status === "Sent" && (
                <button onClick={() => handleStatusChange(selectedInvoice.id, "Paid")} className="w-full bg-green-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-green-700 transition">Ödendi Olarak İşaretle</button>
              )}
              {selectedInvoice.status !== "Cancelled" && selectedInvoice.status !== "Paid" && (
                <button onClick={() => handleStatusChange(selectedInvoice.id, "Cancelled")} className="w-full bg-white text-red-600 border border-red-300 py-2 rounded-lg text-sm font-medium hover:bg-red-50 transition">İptal Et</button>
              )}
              <button onClick={() => handleDelete(selectedInvoice.id)} className="w-full bg-white text-gray-500 border border-gray-300 py-2 rounded-lg text-sm font-medium hover:bg-gray-50 transition">Sil</button>
            </div>
          </div>
        )}
      </div>

      {/* Yeni Fatura Modal */}
      {showForm && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-2xl p-6 w-full max-w-lg max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-bold text-gray-800">Yeni Fatura</h2>
              <button onClick={() => setShowForm(false)}><X size={20} className="text-gray-400" /></button>
            </div>
            <form onSubmit={handleCreate}>
              <div className="grid grid-cols-2 gap-3 mb-3">
                <div>
                  <label className="block text-sm text-gray-600 mb-1">Müşteri</label>
                  <select value={form.customerId} onChange={(e) => setForm({ ...form, customerId: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500">
                    <option value="">Seçiniz</option>
                    {customers.map((c: any) => (
                      <option key={c.id} value={c.id}>{c.fullName}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-sm text-gray-600 mb-1">Vade Tarihi</label>
                  <input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3 mb-4">
                <div>
                  <label className="block text-sm text-gray-600 mb-1">KDV Oranı (%)</label>
                  <input type="number" value={form.taxRate} onChange={(e) => setForm({ ...form, taxRate: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500" />
                </div>
                <div>
                  <label className="block text-sm text-gray-600 mb-1">Notlar</label>
                  <input value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} className="w-full px-4 py-3 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500" placeholder="Opsiyonel" />
                </div>
              </div>

              {/* Kalemler */}
              <div className="mb-4">
                <div className="flex items-center justify-between mb-2">
                  <h3 className="text-sm font-medium text-gray-700">Fatura Kalemleri</h3>
                  <button type="button" onClick={addItem} className="text-blue-600 text-sm font-medium hover:underline">+ Kalem Ekle</button>
                </div>
                {form.items.map((item, index) => (
                  <div key={index} className="flex gap-2 mb-2 items-center">
                    <input placeholder="Açıklama" value={item.description} onChange={(e) => updateItem(index, "description", e.target.value)} className="flex-1 px-3 py-2 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 text-sm" required />
                    <input type="number" placeholder="Adet" value={item.quantity} onChange={(e) => updateItem(index, "quantity", e.target.value)} className="w-16 px-3 py-2 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 text-sm" required />
                    <input type="number" step="0.01" placeholder="Fiyat" value={item.unitPrice} onChange={(e) => updateItem(index, "unitPrice", e.target.value)} className="w-24 px-3 py-2 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-blue-500 text-sm" required />
                    {form.items.length > 1 && (
                      <button type="button" onClick={() => removeItem(index)}><Trash2 size={16} className="text-red-400" /></button>
                    )}
                  </div>
                ))}
              </div>

              {/* Toplam Önizleme */}
              <div className="bg-gray-50 rounded-lg p-3 mb-4 space-y-1">
                <div className="flex justify-between text-sm">
                  <span className="text-gray-500">Ara Toplam</span>
                  <span>₺{getSubTotal().toFixed(2)}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-500">KDV (%{form.taxRate})</span>
                  <span>₺{(getSubTotal() * parseFloat(form.taxRate || "0") / 100).toFixed(2)}</span>
                </div>
                <div className="flex justify-between text-base font-bold border-t border-gray-200 pt-1">
                  <span>Toplam</span>
                  <span className="text-blue-600">₺{(getSubTotal() * (1 + parseFloat(form.taxRate || "0") / 100)).toFixed(2)}</span>
                </div>
              </div>

              <button type="submit" disabled={loading} className="w-full bg-blue-600 text-white py-3 rounded-lg font-medium hover:bg-blue-700 transition disabled:opacity-50">
                {loading ? "Oluşturuluyor..." : "Fatura Oluştur"}
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
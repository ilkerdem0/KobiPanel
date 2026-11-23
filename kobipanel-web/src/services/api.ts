import axios, { AxiosError, InternalAxiosRequestConfig } from "axios";

const BASE_URL = "https://localhost:7245/api";

const api = axios.create({
  baseURL: BASE_URL,
});

// Her isteğe token ekle
api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

const forceLogout = () => {
  localStorage.clear();
  window.location.href = "/login";
};

// Aynı anda birden fazla istek 401 alırsa tek bir refresh isteği atılır,
// diğerleri onun sonucunu bekler.
let refreshPromise: Promise<string> | null = null;

const refreshAccessToken = async (): Promise<string> => {
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) throw new Error("Refresh token yok");

  // Interceptor'lara takılmamak için ayrı axios çağrısı
  const res = await axios.post(`${BASE_URL}/Auth/refresh`, { refreshToken });
  localStorage.setItem("token", res.data.accessToken);
  localStorage.setItem("refreshToken", res.data.refreshToken);
  return res.data.accessToken;
};

type RetriableConfig = InternalAxiosRequestConfig & { _retried?: boolean };

// 401 gelirse token'ı yenileyip isteği bir kez tekrarla; yenileme de başarısızsa logout
api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as RetriableConfig | undefined;
    const isAuthEndpoint = original?.url?.startsWith("/Auth/");

    // Login/register'ın 401'i (hatalı şifre) sayfada mesaj olarak gösterilmeli
    if (error.response?.status !== 401 || !original || isAuthEndpoint) {
      return Promise.reject(error);
    }

    if (original._retried) {
      forceLogout();
      return Promise.reject(error);
    }

    try {
      refreshPromise ??= refreshAccessToken().finally(() => {
        refreshPromise = null;
      });
      const newToken = await refreshPromise;
      original._retried = true;
      original.headers.Authorization = `Bearer ${newToken}`;
      return api(original);
    } catch {
      forceLogout();
      return Promise.reject(error);
    }
  }
);

/** Refresh token'ı sunucuda iptal eder; hata olsa da çıkış devam eder. */
export const revokeRefreshToken = async () => {
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) return;
  try {
    await api.post("/Auth/logout", { refreshToken });
  } catch {}
};

export default api;

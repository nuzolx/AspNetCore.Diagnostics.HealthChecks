import { Liveness, UIApiSettings, WebHook, ApplicationHealthReport } from "../typings/models";
import uiSettings from "../config/UISettings";

export const getHealthChecks = async (): Promise<Liveness[]> => {
  const healthchecksData = await fetch(uiSettings.uiApiEndpoint);
  return healthchecksData.json();
};

export const getUIApiSettings = async (): Promise<UIApiSettings> => {
  const uiApiSettings = await fetch(uiSettings.uiSettingsEndpoint);
  return uiApiSettings.json();
};

export const getWebhooks = async (): Promise<WebHook[]> => {
  const webhooks = await fetch(uiSettings.webhookEndpoint);
  return webhooks.json();
};

export const getApplicationsHealth = async (): Promise<ApplicationHealthReport[]> => {
  const response = await fetch('/api/health/applications');
  
  if (!response.ok) {
    throw new Error(`Applications API returned ${response.status}`);
  }
  
  const data = await response.json();
  
  // Handle both formats: direct array or wrapped in applications property
  if (Array.isArray(data)) {
    return data;
  } else if (data.applications && Array.isArray(data.applications)) {
    return data.applications;
  }
  
  return [];
};

export const getApplicationHealth = async (name: string): Promise<ApplicationHealthReport | null> => {
  const response = await fetch(`/api/health/applications/${encodeURIComponent(name)}`);
  
  if (!response.ok) {
    return null;
  }
  
  return response.json();
};

export default {
  getHealthChecks,
  getUIApiSettings,
  getWebhooks,
  getApplicationsHealth,
  getApplicationHealth,
};

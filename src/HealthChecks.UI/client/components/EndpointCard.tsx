import React, { FunctionComponent } from 'react';
import moment from 'moment';
import { Liveness } from '../typings/models';

interface EndpointCardProps {
  endpoint: Liveness;
  onClick: () => void;
}

const EndpointCard: FunctionComponent<EndpointCardProps> = ({
  endpoint,
  onClick,
}) => {
  const getStatusClass = (status: string): string => {
    const statusLower = status.toLowerCase();
    if (statusLower === 'healthy') return 'healthy';
    if (statusLower === 'unhealthy' || statusLower === 'failed') return 'unhealthy';
    if (statusLower === 'degraded') return 'degraded';
    return 'unknown';
  };

  const getStatusIcon = (status: string): string => {
    const statusLower = status.toLowerCase();
    if (statusLower === 'healthy') return 'check_circle';
    if (statusLower === 'unhealthy' || statusLower === 'failed') return 'error';
    if (statusLower === 'degraded') return 'warning';
    return 'help_outline';
  };

  const statusClass = getStatusClass(endpoint.status);
  const statusIcon = getStatusIcon(endpoint.status);

  const healthyCount = endpoint.entries?.filter(
    (entry) => entry.status.toLowerCase() === 'healthy'
  ).length || 0;
  const totalCount = endpoint.entries?.length || 0;

  return (
    <div
      className={`hc-endpoint-card hc-endpoint-card--${statusClass}`}
      onClick={onClick}
    >
      <div className="hc-endpoint-card__header">
        <div>
          <h3 className="hc-endpoint-card__name">{endpoint.name}</h3>
          <div className="hc-endpoint-card__summary">
            <span className="hc-endpoint-card__summary-text">
              {healthyCount}/{totalCount} checks passing
            </span>
          </div>
        </div>
        <i
          className={`material-icons hc-endpoint-card__status-icon hc-endpoint-card__status-icon--${statusClass}`}
        >
          {statusIcon}
        </i>
      </div>
      <div className="hc-endpoint-card__meta">
        <div className="hc-endpoint-card__timestamp">
          <div>
            <strong>Status:</strong> {endpoint.status}
          </div>
          <div style={{ marginTop: '0.25rem' }}>
            On state from: {moment.utc(endpoint.onStateFrom).fromNow()}
          </div>
        </div>
        <div className="hc-endpoint-card__timestamp">
          Updated: {moment(endpoint.lastExecuted).fromNow()}
        </div>
      </div>
    </div>
  );
};

export { EndpointCard };

import React, { FunctionComponent } from 'react';
import moment from 'moment';
import { Liveness } from '../typings/models';
import { CheckBrick } from './CheckBrick';

interface EndpointDetailModalProps {
  endpoint: Liveness;
  onClose: () => void;
}

const EndpointDetailModal: FunctionComponent<EndpointDetailModalProps> = ({
  endpoint,
  onClose,
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

  const handleOverlayClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  return (
    <div className="hc-modal-overlay" onClick={handleOverlayClick}>
      <div className="hc-modal">
        <div className="hc-modal__header">
          <h2 className="hc-modal__title">{endpoint.name}</h2>
          <button className="hc-modal__close" onClick={onClose}>
            <i className="material-icons">close</i>
          </button>
        </div>
        <div className="hc-modal__body">
          <div className="hc-modal__status-summary">
            <i
              className={`material-icons hc-modal__status-icon hc-endpoint-card__status-icon--${statusClass}`}
            >
              {statusIcon}
            </i>
            <div className="hc-modal__status-details">
              <div className="hc-modal__status-text">
                {endpoint.status} - {healthyCount}/{totalCount} checks passing
              </div>
              <div className="hc-modal__status-time">
                On state from: {moment.utc(endpoint.onStateFrom).fromNow()}
              </div>
              <div className="hc-modal__status-time">
                Last executed: {new Date(endpoint.lastExecuted).toLocaleString()}
              </div>
            </div>
          </div>

          {endpoint.entries && endpoint.entries.length > 0 ? (
            <div>
              <h3 style={{ marginBottom: '1rem', color: 'var(--grayColor)' }}>
                Health Checks ({endpoint.entries.length})
              </h3>
              {endpoint.entries.map((check, index) => (
                <CheckBrick key={index} check={check} />
              ))}
            </div>
          ) : (
            <div style={{ textAlign: 'center', padding: '2rem', color: '#999' }}>
              No health check details available
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export { EndpointDetailModal };

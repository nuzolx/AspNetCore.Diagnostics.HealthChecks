import React, { FunctionComponent } from 'react';
import moment from 'moment';
import { ApplicationHealthReport } from '../typings/models';

interface ApplicationCardProps {
  application: ApplicationHealthReport;
  onClick: () => void;
}

const ApplicationCard: FunctionComponent<ApplicationCardProps> = ({
  application,
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

  const statusClass = getStatusClass(application.status);
  const statusIcon = getStatusIcon(application.status);

  return (
    <div
      className={`hc-endpoint-card hc-endpoint-card--${statusClass}`}
      onClick={onClick}
    >
      <div className="hc-endpoint-card__header">
        <div>
          <h3 className="hc-endpoint-card__name">{application.name}</h3>
          <div className="hc-endpoint-card__summary">
            <span className="hc-endpoint-card__summary-text">
              {application.healthyCount}/{application.totalCount} services OK
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
        <div className="hc-endpoint-card__metrics">
          <div>
            <strong>Status:</strong> {application.status}
          </div>
          <div style={{ marginTop: '0.25rem' }}>
            <strong>Avg latency:</strong> {Math.round(application.averageDurationMs)} ms
          </div>
        </div>
        <div className="hc-endpoint-card__timestamp">
          Updated: {moment(application.checkedAt).fromNow()}
        </div>
      </div>
    </div>
  );
};

export { ApplicationCard };

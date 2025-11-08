import React, { FunctionComponent } from 'react';
import { Check } from '../typings/models';

interface CheckBrickProps {
  check: Check;
}

const CheckBrick: FunctionComponent<CheckBrickProps> = ({ check }) => {
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

  const statusClass = getStatusClass(check.status);
  const statusIcon = getStatusIcon(check.status);

  return (
    <div className={`hc-check-brick hc-check-brick--${statusClass}`}>
      <div className="hc-check-brick__header">
        <div className="hc-check-brick__name">{check.name}</div>
        <div className="hc-check-brick__status">
          <i className={`material-icons hc-check-brick__status-icon hc-endpoint-card__status-icon--${statusClass}`}>
            {statusIcon}
          </i>
          <span>{check.status}</span>
        </div>
      </div>
      {check.description && (
        <div className="hc-check-brick__description">{check.description}</div>
      )}
      <div className="hc-check-brick__footer">
        {check.tags && check.tags.length > 0 && (
          <div className="hc-check-brick__tags">
            {check.tags.map((tag, index) => (
              <span key={index} className="hc-check-brick__tag">
                {tag}
              </span>
            ))}
          </div>
        )}
        {check.duration && (
          <div className="hc-check-brick__duration">
            Duration: {check.duration}
          </div>
        )}
      </div>
    </div>
  );
};

export { CheckBrick };

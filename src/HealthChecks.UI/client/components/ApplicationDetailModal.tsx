import React, { FunctionComponent, useState } from 'react';
import moment from 'moment';
import { ApplicationHealthReport, MemberHealthReport } from '../typings/models';
import ReactJson from 'react-json-view';

interface ApplicationDetailModalProps {
  application: ApplicationHealthReport;
  onClose: () => void;
}

const ApplicationDetailModal: FunctionComponent<ApplicationDetailModalProps> = ({
  application,
  onClose,
}) => {
  const [selectedMember, setSelectedMember] = useState<MemberHealthReport | null>(null);

  const getStatusClass = (status: string): string => {
    const statusLower = status.toLowerCase();
    if (statusLower === 'healthy') return 'healthy';
    if (statusLower === 'unhealthy' || statusLower === 'failed') return 'unhealthy';
    if (statusLower === 'degraded') return 'degraded';
    if (statusLower === 'unreachable') return 'unreachable';
    return 'unknown';
  };

  const getStatusIcon = (status: string): string => {
    const statusLower = status.toLowerCase();
    if (statusLower === 'healthy') return 'check_circle';
    if (statusLower === 'unhealthy' || statusLower === 'failed') return 'error';
    if (statusLower === 'degraded') return 'warning';
    if (statusLower === 'unreachable') return 'cloud_off';
    return 'help_outline';
  };

  const statusClass = getStatusClass(application.status);
  const statusIcon = getStatusIcon(application.status);

  const handleOverlayClick = (e: React.MouseEvent<HTMLDivElement>) => {
    if (e.target === e.currentTarget) {
      onClose();
    }
  };

  const handleMemberPayloadClick = (member: MemberHealthReport) => {
    setSelectedMember(member);
  };

  const handleClosePayload = () => {
    setSelectedMember(null);
  };

  const parsePayload = (payload: string | null) => {
    if (!payload) return null;
    try {
      return JSON.parse(payload);
    } catch {
      return payload;
    }
  };

  return (
    <>
      <div className="hc-modal-overlay" onClick={handleOverlayClick}>
        <div className="hc-modal hc-modal--large">
          <div className="hc-modal__header">
            <h2 className="hc-modal__title">{application.name}</h2>
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
                  {application.status} - {application.healthyCount}/{application.totalCount} services OK
                </div>
                <div className="hc-modal__status-time">
                  Average latency: {Math.round(application.averageDurationMs)} ms
                </div>
                <div className="hc-modal__status-time">
                  Last checked: {new Date(application.checkedAt).toLocaleString()}
                </div>
              </div>
            </div>

            {application.members && application.members.length > 0 ? (
              <div>
                <h3 style={{ marginBottom: '1rem', color: 'var(--grayColor)' }}>
                  Services ({application.members.length})
                </h3>
                <div className="hc-members-list">
                  {application.members.map((member, index) => {
                    const memberStatusClass = getStatusClass(member.status);
                    const memberStatusIcon = getStatusIcon(member.status);
                    
                    return (
                      <div key={index} className={`hc-member-card hc-member-card--${memberStatusClass}`}>
                        <div className="hc-member-card__header">
                          <div className="hc-member-card__info">
                            <div className="hc-member-card__name">
                              <i className={`material-icons hc-member-card__icon hc-member-card__icon--${memberStatusClass}`}>
                                {memberStatusIcon}
                              </i>
                              {member.name}
                            </div>
                            <div className="hc-member-card__status">
                              Status: <span className={`hc-status-badge hc-status-badge--${memberStatusClass}`}>
                                {member.status}
                              </span>
                            </div>
                            <div className="hc-member-card__duration">
                              Duration: {member.durationMs} ms
                            </div>
                          </div>
                          <div className="hc-member-card__actions">
                            <a
                              href={member.uri}
                              target="_blank"
                              rel="noopener noreferrer"
                              className="hc-member-card__link"
                              onClick={(e) => e.stopPropagation()}
                            >
                              <i className="material-icons">open_in_new</i>
                              Open
                            </a>
                            {member.payload && (
                              <button
                                className="hc-member-card__button"
                                onClick={() => handleMemberPayloadClick(member)}
                              >
                                <i className="material-icons">code</i>
                                View Payload
                              </button>
                            )}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            ) : (
              <div style={{ textAlign: 'center', padding: '2rem', color: '#999' }}>
                No service details available
              </div>
            )}
          </div>
        </div>
      </div>

      {selectedMember && (
        <div className="hc-modal-overlay" onClick={handleClosePayload}>
          <div className="hc-modal hc-modal--payload">
            <div className="hc-modal__header">
              <h3 className="hc-modal__title">Payload: {selectedMember.name}</h3>
              <button className="hc-modal__close" onClick={handleClosePayload}>
                <i className="material-icons">close</i>
              </button>
            </div>
            <div className="hc-modal__body">
              {typeof parsePayload(selectedMember.payload) === 'object' ? (
                <ReactJson
                  src={parsePayload(selectedMember.payload)}
                  theme="rjv-default"
                  collapsed={false}
                  displayDataTypes={false}
                  displayObjectSize={false}
                  enableClipboard={true}
                />
              ) : (
                <pre className="hc-payload-text">{selectedMember.payload}</pre>
              )}
            </div>
          </div>
        </div>
      )}
    </>
  );
};

export { ApplicationDetailModal };

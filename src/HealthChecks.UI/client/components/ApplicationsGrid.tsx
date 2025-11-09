import React, { FunctionComponent, useState } from 'react';
import { ApplicationHealthReport } from '../typings/models';
import { ApplicationCard } from './ApplicationCard';
import { ApplicationDetailModal } from './ApplicationDetailModal';

interface ApplicationsGridProps {
  applications: Array<ApplicationHealthReport>;
}

const ApplicationsGrid: FunctionComponent<ApplicationsGridProps> = ({ applications }) => {
  const [selectedApplication, setSelectedApplication] = useState<ApplicationHealthReport | null>(null);

  const handleCardClick = (application: ApplicationHealthReport) => {
    setSelectedApplication(application);
  };

  const handleCloseModal = () => {
    setSelectedApplication(null);
  };

  if (!applications || applications.length === 0) {
    return (
      <div className="hc-grid-empty">
        <i className="material-icons hc-grid-empty__icon">healing</i>
        <p className="hc-grid-empty__message">No applications available</p>
      </div>
    );
  }

  return (
    <>
      <div className="hc-endpoint-grid">
        {applications.map((application, index) => (
          <ApplicationCard
            key={index}
            application={application}
            onClick={() => handleCardClick(application)}
          />
        ))}
      </div>
      {selectedApplication && (
        <ApplicationDetailModal
          application={selectedApplication}
          onClose={handleCloseModal}
        />
      )}
    </>
  );
};

export { ApplicationsGrid };

import React, { FunctionComponent, useState } from 'react';
import { ApplicationHealthReport, Liveness } from '../typings/models';
import { ApplicationCard } from './ApplicationCard';
import { ApplicationDetailModal } from './ApplicationDetailModal';
import { EndpointCard } from './EndpointCard';
import { EndpointDetailModal } from './EndpointDetailModal';

interface ApplicationsGridProps {
  applications: Array<ApplicationHealthReport>;
  ungroupedHealthChecks: Array<Liveness>;
}

const ApplicationsGrid: FunctionComponent<ApplicationsGridProps> = ({ 
  applications, 
  ungroupedHealthChecks 
}) => {
  const [selectedApplication, setSelectedApplication] = useState<ApplicationHealthReport | null>(null);
  const [selectedEndpoint, setSelectedEndpoint] = useState<Liveness | null>(null);

  const handleCardClick = (application: ApplicationHealthReport) => {
    setSelectedApplication(application);
  };

  const handleEndpointClick = (endpoint: Liveness) => {
    setSelectedEndpoint(endpoint);
  };

  const handleCloseModal = () => {
    setSelectedApplication(null);
  };

  const handleCloseEndpointModal = () => {
    setSelectedEndpoint(null);
  };

  if ((!applications || applications.length === 0) && (!ungroupedHealthChecks || ungroupedHealthChecks.length === 0)) {
    return (
      <div className="hc-grid-empty">
        <i className="material-icons hc-grid-empty__icon">healing</i>
        <p className="hc-grid-empty__message">No applications or health checks available</p>
      </div>
    );
  }

  return (
    <>
      <div className="hc-endpoint-grid">
        {applications && applications.map((application, index) => (
          <ApplicationCard
            key={`app-${index}`}
            application={application}
            onClick={() => handleCardClick(application)}
          />
        ))}
        {ungroupedHealthChecks && ungroupedHealthChecks.map((endpoint, index) => (
          <EndpointCard
            key={`endpoint-${index}`}
            endpoint={endpoint}
            onClick={() => handleEndpointClick(endpoint)}
          />
        ))}
      </div>
      {selectedApplication && (
        <ApplicationDetailModal
          application={selectedApplication}
          onClose={handleCloseModal}
        />
      )}
      {selectedEndpoint && (
        <EndpointDetailModal
          endpoint={selectedEndpoint}
          onClose={handleCloseEndpointModal}
        />
      )}
    </>
  );
};

export { ApplicationsGrid };

import React, { FunctionComponent, useState } from 'react';
import { Liveness } from '../typings/models';
import { EndpointCard } from './EndpointCard';
import { EndpointDetailModal } from './EndpointDetailModal';

interface EndpointGridProps {
  livenessData: Array<Liveness>;
}

const EndpointGrid: FunctionComponent<EndpointGridProps> = ({ livenessData }) => {
  const [selectedEndpoint, setSelectedEndpoint] = useState<Liveness | null>(null);

  const handleCardClick = (endpoint: Liveness) => {
    setSelectedEndpoint(endpoint);
  };

  const handleCloseModal = () => {
    setSelectedEndpoint(null);
  };

  if (!livenessData || livenessData.length === 0) {
    return (
      <div className="hc-grid-empty">
        <i className="material-icons hc-grid-empty__icon">healing</i>
        <p className="hc-grid-empty__message">No health checks available</p>
      </div>
    );
  }

  return (
    <>
      <div className="hc-endpoint-grid">
        {livenessData.map((endpoint, index) => (
          <EndpointCard
            key={index}
            endpoint={endpoint}
            onClick={() => handleCardClick(endpoint)}
          />
        ))}
      </div>
      {selectedEndpoint && (
        <EndpointDetailModal
          endpoint={selectedEndpoint}
          onClose={handleCloseModal}
        />
      )}
    </>
  );
};

export { EndpointGrid };

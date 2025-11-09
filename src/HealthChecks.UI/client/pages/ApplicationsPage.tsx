import React, { useState, useEffect } from 'react';
import { UIApiSettings } from '../typings/models';
import { ApplicationsGrid } from '../components/ApplicationsGrid';
import { useQuery } from 'react-query';
import { getApplicationsHealth } from '../api/fetchers';
import { LivenessMenu } from '../components/LivenessMenu';
import { AlertPanel } from '../components/AlertPanel';

interface ApplicationsPageProps {
  apiSettings: UIApiSettings;
}

const ApplicationsPage: React.FunctionComponent<ApplicationsPageProps> = ({
  apiSettings,
}) => {
  const [fetchInterval, setFetchInterval] = useState<number | false>(
    apiSettings.pollingInterval * 1000
  );
  const [running, setRunning] = useState<boolean>(true);

  const { data: applicationsData, isError, error } = useQuery(
    'applications',
    getApplicationsHealth,
    { 
      refetchInterval: fetchInterval, 
      keepPreviousData: true, 
      retry: 1,
      // Don't throw on error, we want to handle it gracefully
      useErrorBoundary: false
    }
  );

  useEffect(() => {
    console.log(`Configured polling interval: ${fetchInterval} milliseconds`);
  }, []);

  useEffect(() => {
    if (!running) {
      setFetchInterval(false);
      return;
    }
    setFetchInterval(apiSettings.pollingInterval * 1000);
  }, [running]);

  return (
    <article className="hc-liveness">
      <header className="hc-liveness__header">
        <h1>{apiSettings.headerText}</h1>
        <LivenessMenu
          pollingInterval={apiSettings.pollingInterval}
          running={running}
          onRunningClick={() => setRunning(!running)}
        />
      </header>
      {isError ? (
        <AlertPanel message="Applications view is not available. Please check your configuration or use the standard Health Checks view." />
      ) : null}
      <div className="hc-grid-container">
        {applicationsData !== undefined ? (
          <ApplicationsGrid applications={applicationsData!} />
        ) : null}
      </div>
    </article>
  );
};

export { ApplicationsPage };

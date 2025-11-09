import React, { useState, useEffect } from 'react';
import { UIApiSettings, Liveness } from '../typings/models';
import { ApplicationsGrid } from '../components/ApplicationsGrid';
import { useQuery } from 'react-query';
import { getApplicationsHealth, getHealthChecks } from '../api/fetchers';
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

  const { data: applicationsData, isError: isApplicationsError } = useQuery(
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

  const { data: healthChecksData, isError: isHealthChecksError } = useQuery(
    'healthchecks',
    getHealthChecks,
    { 
      refetchInterval: fetchInterval, 
      keepPreviousData: true, 
      retry: 1,
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

  // Find health checks that are not part of any application
  const getUngroupedHealthChecks = (): Liveness[] => {
    if (!healthChecksData || !applicationsData) return [];

    // Get all member names from all applications
    const groupedMemberNames = new Set<string>();
    applicationsData.forEach(app => {
      app.members.forEach(member => {
        groupedMemberNames.add(member.name);
      });
    });

    // Filter health checks that are not in any application
    return healthChecksData.filter(hc => !groupedMemberNames.has(hc.name));
  };

  const ungroupedHealthChecks = getUngroupedHealthChecks();

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
      {isApplicationsError ? (
        <AlertPanel message="Applications view is not available. Please check your configuration or use the standard Health Checks view." />
      ) : null}
      <div className="hc-grid-container">
        {applicationsData !== undefined || ungroupedHealthChecks.length > 0 ? (
          <ApplicationsGrid 
            applications={applicationsData || []} 
            ungroupedHealthChecks={ungroupedHealthChecks}
          />
        ) : null}
      </div>
    </article>
  );
};

export { ApplicationsPage };

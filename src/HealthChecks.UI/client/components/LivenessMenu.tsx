import React, { FunctionComponent } from "react";
import { useHistory, useLocation } from 'react-router-dom';

interface LivenessMenuProps {
    pollingInterval: number,
    running: boolean
    onRunningClick: () => void;
}

type ViewType = 'table' | 'grid' | 'applications';

const LivenessMenu: FunctionComponent<LivenessMenuProps> = ({ running, onRunningClick, pollingInterval }) => {
    const history = useHistory();
    const location = useLocation();
    
    // Determine current view type based on path
    const getCurrentView = (): ViewType => {
        if (location.pathname === '/healthchecks-grid') return 'grid';
        if (location.pathname === '/applications') return 'applications';
        return 'table';
    };

    const currentView = getCurrentView();

    const cycleView = () => {
        // Cycle through: table → grid → applications → table
        switch (currentView) {
            case 'table':
                history.push('/healthchecks-grid');
                break;
            case 'grid':
                history.push('/applications');
                break;
            case 'applications':
                history.push('/healthchecks');
                break;
        }
    };

    const getViewIcon = (view: ViewType): string => {
        switch (view) {
            case 'table': return 'view_list';
            case 'grid': return 'grid_view';
            case 'applications': return 'apps';
        }
    };

    const getViewLabel = (view: ViewType): string => {
        switch (view) {
            case 'table': return 'Table View';
            case 'grid': return 'Grid View';
            case 'applications': return 'Applications';
        }
    };

    // Get next view for button label
    const getNextView = (): ViewType => {
        switch (currentView) {
            case 'table': return 'grid';
            case 'grid': return 'applications';
            case 'applications': return 'table';
        }
    };

    const nextView = getNextView();

    return (
        <div className="hc-refesh-group">
            <span>Polling interval: <b>{pollingInterval}</b> secs</span>
            <div className="hc-view-toggle">
                <button
                    onClick={cycleView}
                    type="button"
                    className="hc-view-toggle__button"
                    title={`Switch to ${getViewLabel(nextView)}`}>
                    <i className="material-icons" style={{ fontSize: '1rem', verticalAlign: 'middle', marginRight: '0.25rem' }}>
                        {getViewIcon(nextView)}
                    </i>
                    {getViewLabel(nextView)}
                </button>
                <button
                    onClick={onRunningClick}
                    type="button"
                    className="hc-button">
                    {running ? "Stop polling" : "Start polling"}
                </button>
            </div>
        </div>)
};

export { LivenessMenu };
